<#
    ServiCore - second trial organization (Lumen Peak Software)
    =============================================================
    Separate company, separate people, separate email domain from the
    Cascade Freight trial - so this can run against the same live database
    with zero collisions (ASP.NET Identity emails are unique system-wide,
    not per-organization).

    EMAIL - uses mailinator.com
    -----------------------------
    Every invited address below is @mailinator.com. Mailinator is a public,
    no-signup inbox: your API's real SMTP will really deliver to it, and
    you read it by opening https://www.mailinator.com and typing the inbox
    name (e.g. "lumenpeak-manager") - no login needed. Don't put anything
    sensitive through it; that's fine here, it's disposable demo data.

    ABOUT "ONE TRANSACTION" - please read before running
    -------------------------------------------------------
    You asked for this to behave like a single transaction: if anything
    fails, nothing should remain in the database. I looked through the
    actual API and that's not fully achievable, and I'd rather tell you
    that plainly than have the script silently pretend otherwise:

      - There is NO delete endpoint for an Organization, and NO endpoint
        to delete a user account, anywhere in this API. Once step 1
        (register) succeeds, the organization and its owner exist
        permanently - no script can undo that part.
      - There is NO delete endpoint for a Ticket. Once a ticket is
        created, it's permanent, even if a later ticket in the same run
        fails.
      - Categories, Teams, and Customers DO have a working deactivate
        endpoint, Team membership DOES have a remove endpoint, and a
        pending (not-yet-accepted) Invitation DOES have a revoke endpoint.

    So what this script actually gives you:
      1. FAIL-FAST - the instant any step errors, it stops. It will not
         keep going and create more data on top of a failure.
      2. AUTO-ROLLBACK of everything that *can* be undone (categories,
         teams, team memberships, customers, still-pending invitations),
         in reverse order, the moment a failure is caught.
      3. A clear final report naming anything that could NOT be rolled
         back, so you always know exactly what's left in the database -
         never a silent partial state.

    In practice: a failure during categories/teams/customers/invites
    cleans up to nothing. The real residual risk is the tickets phase -
    once ticket #1 exists, a failure on ticket #2 leaves ticket #1 behind
    permanently. That's a gap in the API, not something PowerShell can
    close; if you want a true hard reset later, the real fix is a
    dev-only "wipe this organization" endpoint on the backend - happy to
    build that if you want it, just ask.

    Usage:
        .\servicore-seed-trial-lumenpeak.ps1
    (Works the same in Visual Studio's integrated PowerShell terminal,
    plain PowerShell 5.1, or PowerShell 7+ - it's the same engine either
    way, nothing VS-specific needed.)
#>

$ErrorActionPreference = "Stop"

# ============================================================================
# CONFIG
# ============================================================================
# The server root ONLY - no "/api" here. Every request path below already starts with "/api/...",
# so putting "/api" in this value made every call go to /api/api/... and return 404.
$BaseUrl        = "https://servicore.runasp.net"
# Safety net: if someone pastes the API URL with a trailing slash or a trailing /api, normalize it.
$BaseUrl        = ($BaseUrl.TrimEnd('/') -replace '/api$', '')
$SharedPassword = "ServiCore#2025"
$OrgName        = "Lumen Peak Software"

# The first (failed) run already created the organization + owner, and those can't be deleted.
# Setting this makes the script SKIP registration and just log in as the existing owner.
# Set it to "" for a brand-new organization.
$ExistingOrganizationId = "310dab1f-3ade-4720-97fb-f34e66407ae2"

# Mailinator is blocked for you (Cloudflare), so invitations go to YOUR real inbox instead.
# Gmail/Outlook "plus addressing": name+tag@gmail.com is a DIFFERENT address to ServiCore
# (so each person gets their own account) but every email lands in your one inbox.
# Put your real address here (must support +tags, e.g. Gmail, Outlook.com, iCloud).
$InboxEmail = "ahmedmohamedwebdev@gmail.com"

# If you ever need brand-new accounts (e.g. you forgot the password of an already-activated one -
# the API has no password-reset endpoint), set this to e.g. "-b" to get fresh +tag addresses.
$TagSuffix = "-b"

function New-InboxAddress([string]$tag) {
    $parts = $InboxEmail.Split('@')
    return ("{0}+{1}{2}@{3}" -f $parts[0], $tag, $TagSuffix, $parts[1]).ToLowerInvariant()
}

# Follows redirects by hand (without opening a browser) and returns the first URL that contains "token=".
# Needed because Brevo wraps the Accept button in a click-tracking link (sendibt2.com/tr/cl/...)
# that redirects to the real  <frontend>/accept-invitation?token=...  URL.
function Resolve-TokenUrl([string]$url) {
    for ($hop = 0; $hop -lt 10 -and $url -notmatch 'token='; $hop++) {
        $loc = $null
        try {
            Invoke-WebRequest -Uri $url -MaximumRedirection 0 -UseBasicParsing -ErrorAction Stop | Out-Null
            break   # 200 OK, no further redirect
        } catch {
            $resp = $_.Exception.Response
            if (-not $resp) { break }
            try { $loc = $resp.Headers.Location } catch { }
            if (-not $loc) { try { $loc = $resp.Headers["Location"] } catch { } }
        }
        if (-not $loc) { break }
        $url = ([System.Uri]::new([System.Uri]$url, [string]$loc)).AbsoluteUri
    }
    return $url
}

# Accepts the bare token, the real accept-invitation link, OR the Brevo tracking link.
# Un-escapes %2B / %2F / %3D. Keeps asking until it can find a token.
function Read-InviteToken([string]$prompt) {
    while ($true) {
        $raw = (Read-Host $prompt).Trim()
        if ($raw -match '^https?://' -and $raw -notmatch 'token=') {
            Write-Host "  (tracking link detected - following redirects to find the real invitation URL...)" -ForegroundColor DarkGray
            $raw = Resolve-TokenUrl $raw
        }
        if ($raw -match 'token=([^&\s]+)') { $raw = $Matches[1] }
        elseif ($raw -match '^https?://') {
            Write-Host "  Could not find a token in that link. Open it in your browser, wait for the page to load," -ForegroundColor Red
            Write-Host "  copy the address bar (it should contain ...accept-invitation?token=...) and paste THAT here." -ForegroundColor Red
            continue
        }
        return [System.Uri]::UnescapeDataString($raw).Trim()
    }
}

$OrganizationRole = @{ Manager = 2; Agent = 3 }
$TicketPriority   = @{ Low = 1; Medium = 2; High = 3; Critical = 4 }

# ----------------------------------------------------------------------------
# Tracks everything created this run so a failure can be rolled back.
# ----------------------------------------------------------------------------
$Created = @{
    categories          = New-Object System.Collections.Generic.List[object]  # @{ id; name }
    teams               = New-Object System.Collections.Generic.List[object]  # @{ id; name }
    customers           = New-Object System.Collections.Generic.List[object]  # @{ id; name }
    staffInvitations    = New-Object System.Collections.Generic.List[object]  # @{ id; email; accepted }
    customerInvitations = New-Object System.Collections.Generic.List[object]  # @{ id; email; accepted }
    teamMemberships     = New-Object System.Collections.Generic.List[object]  # @{ teamId; userId }
    tickets             = New-Object System.Collections.Generic.List[object]  # @{ id; title }  -- NOT reversible
}
$OrganizationCreated = $false

function Invoke-Api {
    param(
        [Parameter(Mandatory)] [string]$Method,
        [Parameter(Mandatory)] [string]$Path,
        [hashtable]$Body,
        [string]$Token,
        [string]$OrganizationId
    )
    $headers = @{}
    if ($Token)          { $headers["Authorization"]     = "Bearer $Token" }
    if ($OrganizationId) { $headers["X-Organization-Id"] = $OrganizationId }

    $params = @{
        Method      = $Method
        Uri         = "$BaseUrl$Path"
        Headers     = $headers
        ContentType = "application/json"
    }
    if ($Body) { $params["Body"] = ($Body | ConvertTo-Json -Depth 6) }

    return Invoke-RestMethod @params
}

function Section([string]$text) {
    Write-Host ""
    Write-Host "== $text ==" -ForegroundColor Cyan
}

function Write-ApiError($err, [string]$context) {
    Write-Host ""
    Write-Host "FAILED: $context" -ForegroundColor Red
    $detail = $err.ErrorDetails.Message
    if (-not $detail -and $err.Exception.Response) {
        try {
            $stream = $err.Exception.Response.GetResponseStream()
            $stream.Position = 0
            $detail = (New-Object System.IO.StreamReader($stream)).ReadToEnd()
        } catch { }
    }
    Write-Host $err.Exception.Message -ForegroundColor Red
    if ($detail) { Write-Host "Server said: $detail" -ForegroundColor Red }
}

# ============================================================================
# Rollback - best effort, undoes everything that has an undo path.
# ============================================================================
function Invoke-Rollback {
    param([string]$OwnerToken, [string]$OrganizationId)

    Section "Rolling back everything that CAN be undone"

    if (-not $OrganizationId) {
        Write-Host "  Organization was never created - nothing to roll back." -ForegroundColor Yellow
        return
    }

    # 1. Team memberships
    foreach ($m in $Created.teamMemberships) {
        try {
            Invoke-Api -Method DELETE -Path "/api/teams/$($m.teamId)/members/$($m.userId)" -Token $OwnerToken -OrganizationId $OrganizationId | Out-Null
            Write-Host "  Removed team membership $($m.userId) from team $($m.teamId)"
        } catch { Write-Host "  Could not remove team membership $($m.userId)/$($m.teamId): $($_.Exception.Message)" -ForegroundColor DarkYellow }
    }

    # 2. Customer invitations - the API never returns an invitation id for
    #    these (no GET/list endpoint for them either), so there is no way
    #    to call revoke at all, accepted or not. Report, don't pretend.
    foreach ($inv in $Created.customerInvitations) {
        if ($inv.accepted) {
            Write-Host "  Customer invite for $($inv.email) was already accepted - account persists." -ForegroundColor DarkYellow
        } else {
            Write-Host "  Customer invite for $($inv.email) is pending but can't be revoked (API never exposes its id) - will sit as a valid pending invite." -ForegroundColor DarkYellow
        }
    }

    # 3. Customers
    foreach ($c in $Created.customers) {
        try {
            Invoke-Api -Method DELETE -Path "/api/customers/$($c.id)" -Token $OwnerToken -OrganizationId $OrganizationId | Out-Null
            Write-Host "  Deactivated customer $($c.name)"
        } catch { Write-Host "  Could not deactivate customer $($c.name): $($_.Exception.Message)" -ForegroundColor DarkYellow }
    }

    # 4. Pending staff invitations (accepted ones can't be revoked - skip, report)
    foreach ($inv in $Created.staffInvitations) {
        if ($inv.accepted) {
            Write-Host "  Staff invite for $($inv.email) was already accepted - cannot revoke, account persists." -ForegroundColor DarkYellow
            continue
        }
        try {
            Invoke-Api -Method POST -Path "/api/organization/invitations/$($inv.id)/revoke" -Token $OwnerToken -OrganizationId $OrganizationId | Out-Null
            Write-Host "  Revoked pending staff invitation for $($inv.email)"
        } catch { Write-Host "  Could not revoke staff invitation for $($inv.email): $($_.Exception.Message)" -ForegroundColor DarkYellow }
    }

    # 5. Teams
    foreach ($t in $Created.teams) {
        try {
            Invoke-Api -Method DELETE -Path "/api/teams/$($t.id)" -Token $OwnerToken -OrganizationId $OrganizationId | Out-Null
            Write-Host "  Deactivated team $($t.name)"
        } catch { Write-Host "  Could not deactivate team $($t.name): $($_.Exception.Message)" -ForegroundColor DarkYellow }
    }

    # 6. Categories
    foreach ($c in $Created.categories) {
        try {
            Invoke-Api -Method DELETE -Path "/api/categories/$($c.id)" -Token $OwnerToken -OrganizationId $OrganizationId | Out-Null
            Write-Host "  Deactivated category $($c.name)"
        } catch { Write-Host "  Could not deactivate category $($c.name): $($_.Exception.Message)" -ForegroundColor DarkYellow }
    }

    Write-Host ""
    Write-Host "CANNOT be undone (no delete endpoint exists for these):" -ForegroundColor Red
    Write-Host "  - The organization '$OrgName' itself and its owner account."
    if ($Created.tickets.Count -gt 0) {
        Write-Host "  - $($Created.tickets.Count) ticket(s) already created:"
        foreach ($t in $Created.tickets) { Write-Host "      $($t.title)" }
    } else {
        Write-Host "  - (no tickets had been created yet, so none are stuck)"
    }
    $acceptedStaff = $Created.staffInvitations | Where-Object { $_.accepted }
    if ($acceptedStaff) {
        Write-Host "  - Staff account(s) already activated:"
        foreach ($s in $acceptedStaff) { Write-Host "      $($s.email)" }
    }
    $acceptedCust = $Created.customerInvitations | Where-Object { $_.accepted }
    if ($acceptedCust) {
        Write-Host "  - Customer portal account(s) already activated:"
        foreach ($c in $acceptedCust) { Write-Host "      $($c.email)" }
    }
}

# ============================================================================
# MAIN
# ============================================================================
if ($InboxEmail -like "PUT_YOUR_REAL_EMAIL*") {
    Write-Host "Edit the script first: set `$InboxEmail near the top to your real email address." -ForegroundColor Red
    return
}

try {

    # ------------------------------------------------------------------------
    # 1. Register organization + owner (irreversible from here on)
    # ------------------------------------------------------------------------
    Section "Registering organization and owner"

    $owner = @{
        name  = "Marcus Webb"
        email = "marcus.webb@lumenpeak-demo.local"
    }

    if ($ExistingOrganizationId) {
        $OrganizationId = $ExistingOrganizationId
        Write-Host "  Reusing existing organization $OrgName ($OrganizationId)"
    } else {
        $registerResult = Invoke-Api -Method POST -Path "/api/auth/register" -Body @{
            name             = $owner.name
            email            = $owner.email
            password         = $SharedPassword
            organizationName = $OrgName
        }
        $OrganizationCreated = $true
        $OrganizationId = $registerResult.organizationId
        Write-Host "  Organization: $OrgName ($OrganizationId)"
    }
    Write-Host "  Owner login : $($owner.email) / $SharedPassword"

    $ownerLogin = Invoke-Api -Method POST -Path "/api/auth/login" -Body @{
        email    = $owner.email
        password = $SharedPassword
    }
    $OwnerToken = $ownerLogin.token

    # ------------------------------------------------------------------------
    # 2. Categories
    # ------------------------------------------------------------------------
    Section "Creating categories"

    $categoryNames = @(
        @{ name = "Bug Report";              description = "Something in the product is broken or behaving incorrectly." },
        @{ name = "API / Integration Issue";  description = "Webhooks, API keys, and third-party connectors." },
        @{ name = "Billing Question";         description = "Invoices, charges, seats, plan changes." },
        @{ name = "Onboarding Help";          description = "New account setup, data migration, initial configuration." },
        @{ name = "Feature Request";          description = "Ideas and enhancement requests from customers." },
        @{ name = "Performance Issue";        description = "Slowness, timeouts, degraded response times." }
    )

    $Categories = @{}
    # Reuse categories that already exist (e.g. left behind by an earlier failed run) -
    # the API rejects duplicate names with a 400.
    $existingCategories = @(Invoke-Api -Method GET -Path "/api/categories" -Token $OwnerToken -OrganizationId $OrganizationId)
    foreach ($c in $categoryNames) {
        $match = $existingCategories | Where-Object { $_.name -eq $c.name } | Select-Object -First 1
        if ($match) {
            $Categories[$c.name] = $match.id
            Write-Host "  = $($c.name) (already exists, reusing)"
            continue
        }
        $apiResult = Invoke-Api -Method POST -Path "/api/categories" -Token $OwnerToken -OrganizationId $OrganizationId -Body @{
            name        = $c.name
            description = $c.description
        }
        $Categories[$c.name] = $apiResult.id
        $Created.categories.Add(@{ id = $apiResult.id; name = $c.name })
        Write-Host "  + $($c.name)"
    }

    # ------------------------------------------------------------------------
    # 3. Teams
    # ------------------------------------------------------------------------
    Section "Creating teams"

    $teamNames = @(
        @{ name = "Platform Support";   description = "Core product issues and bugs reported by customers." },
        @{ name = "Integrations Team";  description = "API, webhooks and third-party connector issues." },
        @{ name = "Customer Success";   description = "Onboarding, billing and account management." }
    )

    $Teams = @{}
    # The backend has no duplicate check for teams (a repeat name = HTTP 500 from the DB unique index),
    # so reuse any team that already exists.
    $existingTeams = @(Invoke-Api -Method GET -Path "/api/teams" -Token $OwnerToken -OrganizationId $OrganizationId)
    foreach ($t in $teamNames) {
        $match = $existingTeams | Where-Object { $_.name -eq $t.name } | Select-Object -First 1
        if ($match) {
            $Teams[$t.name] = $match.id
            Write-Host "  = $($t.name) (already exists, reusing)"
            continue
        }
        $apiResult = Invoke-Api -Method POST -Path "/api/teams" -Token $OwnerToken -OrganizationId $OrganizationId -Body @{
            name        = $t.name
            description = $t.description
        }
        $Teams[$t.name] = $apiResult.id
        $Created.teams.Add(@{ id = $apiResult.id; name = $t.name })
        Write-Host "  + $($t.name)"
    }

    # ------------------------------------------------------------------------
    # 4. Invite staff - emails go to your real inbox via +tags
    # ------------------------------------------------------------------------
    Section "Inviting staff"

    $staff = @(
        @{ key = "manager"; name = "Elena Kowalski"; email = (New-InboxAddress "lumenpeak-manager"); role = $OrganizationRole.Manager }
        @{ key = "agent1";  name = "Dev Patel";       email = (New-InboxAddress "lumenpeak-agent1");  role = $OrganizationRole.Agent }
        @{ key = "agent2";  name = "Sofia Marin";      email = (New-InboxAddress "lumenpeak-agent2");  role = $OrganizationRole.Agent }
    )

    # Look at what already exists from earlier runs.
    $existingInvites = @(Invoke-Api -Method GET -Path "/api/organization/invitations" -Token $OwnerToken -OrganizationId $OrganizationId)
    $wanted = $staff | ForEach-Object { $_.email }

    # Revoke leftover PENDING invitations: the old unreadable mailinator ones, and any pending
    # one for an address we are about to invite (the token can't be re-sent, so we re-issue).
    foreach ($i in $existingInvites) {
        $isPending = (-not $i.isAccepted) -and (-not $i.isRevoked)
        if ($isPending -and (($wanted -contains $i.email) -or ($i.email -like "lumenpeak-*@mailinator.com"))) {
            Invoke-Api -Method POST -Path "/api/organization/invitations/$($i.id)/revoke" -Token $OwnerToken -OrganizationId $OrganizationId | Out-Null
            Write-Host "  Revoked stale pending invitation for $($i.email)"
        }
    }
    $alreadyAccepted = @($existingInvites | Where-Object { $_.isAccepted } | ForEach-Object { $_.email })

    foreach ($p in $staff) {
        if ($alreadyAccepted -contains $p.email) {
            Write-Host "  $($p.name) <$($p.email)> already accepted - skipping invite"
            continue
        }
        $inv = Invoke-Api -Method POST -Path "/api/organization/invitations" -Token $OwnerToken -OrganizationId $OrganizationId -Body @{
            email = $p.email
            role  = $p.role
        }
        $Created.staffInvitations.Add(@{ id = $inv.invitation.id; email = $p.email; accepted = $false })
        Write-Host "  Invited $($p.name) <$($p.email)>"
    }

    $pendingStaff = @($staff | Where-Object { $alreadyAccepted -notcontains $_.email })
    if ($pendingStaff.Count -gt 0) {
        Write-Host ""
        Write-Host "Open the inbox of $InboxEmail (check Spam too). You'll get $($pendingStaff.Count) invitation email(s)." -ForegroundColor Yellow
        Write-Host "Each has an 'Accept Invitation' button linking to .../accept-invitation?token=THE_TOKEN" -ForegroundColor Yellow
        Write-Host "Right-click the button > Copy link address, then right-click in this window to PASTE the whole link." -ForegroundColor Yellow
        Write-Host "(Do NOT press Ctrl+C in this window - it aborts the script. Copy inside the browser/mail app only.)" -ForegroundColor Yellow
        Write-Host "The emails say which role they are for: Manager = Elena, the two Agent emails = Dev then Sofia (match by the + tag in 'To')." -ForegroundColor Yellow
    }

    $Users = @{}
    foreach ($p in $staff) {
        if ($alreadyAccepted -notcontains $p.email) {
            $token = Read-InviteToken "Link or token for $($p.name) <$($p.email)>"
            Invoke-Api -Method POST -Path "/api/organization/invitations/accept" -Body @{
                token    = $token
                password = $SharedPassword
            } | Out-Null
            ($Created.staffInvitations | Where-Object { $_.email -eq $p.email }).accepted = $true
        }

        # An account activated in the BROWSER (by clicking the email's Accept button) has whatever
        # password you typed there, not the shared one - so on a 401 ask for it instead of dying.
        $pw = $SharedPassword
        while ($true) {
            try {
                $login = Invoke-Api -Method POST -Path "/api/auth/login" -Body @{
                    email    = $p.email
                    password = $pw
                }
                break
            } catch {
                $status = $null
                if ($_.Exception.Response) { $status = [int]$_.Exception.Response.StatusCode }
                if ($status -ne 401) { throw }
                Write-Host "  Login failed for $($p.email) - it was probably activated in the browser with a different password." -ForegroundColor Yellow
                $pw = Read-Host "  Type the password you chose for $($p.email) (just press Enter to abort)"
                if (-not $pw) { throw }
            }
        }
        $me = Invoke-Api -Method GET -Path "/api/auth/me" -Token $login.token

        $Users[$p.key] = @{ userId = $me.userId; token = $login.token; name = $p.name; email = $p.email; password = $pw }
        Write-Host "  Activated $($p.name) ($($me.userId))" -ForegroundColor Green
    }

    # ------------------------------------------------------------------------
    # 5. Put staff on teams
    # ------------------------------------------------------------------------
    Section "Assigning staff to teams"

    $teamAssignments = @(
        @{ team = "Platform Support";  userKey = "manager" }
        @{ team = "Platform Support";  userKey = "agent1" }
        @{ team = "Integrations Team"; userKey = "agent2" }
        @{ team = "Customer Success";  userKey = "agent2" }
    )

    foreach ($a in $teamAssignments) {
        $teamId = $Teams[$a.team]
        $userId = $Users[$a.userKey].userId
        Invoke-Api -Method POST -Path "/api/teams/$teamId/members" -Token $OwnerToken -OrganizationId $OrganizationId -Body @{
            userId = $userId
        } | Out-Null
        $Created.teamMemberships.Add(@{ teamId = $teamId; userId = $userId })
        Write-Host "  $($Users[$a.userKey].name) -> $($a.team)"
    }

    # ------------------------------------------------------------------------
    # 6. Customers
    # ------------------------------------------------------------------------
    Section "Creating customers"

    $customerDefs = @(
        @{ key = "nimbus";     name = "Nimbus Retail Group (Alicia Ford)";     email = "$(New-InboxAddress "lumenpeak-cust-nimbus")"; phone = "+1 555 0301" }
        @{ key = "vertex";     name = "Vertex Manufacturing (Grace Liu)";      email = "$(New-InboxAddress "lumenpeak-cust-vertex")"; phone = "+1 555 0322" }
        @{ key = "clearwater"; name = "Clearwater Logistics (Omar Haidari)";   email = "clearwater.ops@example.com";           phone = "+1 555 0345" }
        @{ key = "pinecrest";  name = "Pinecrest Realty Group (Natalie Brooks)"; email = "natalie@pinecrestrealty.example";    phone = "+1 555 0367" }
        @{ key = "solace";     name = "Solace Health Partners (Daniel Okafor)"; email = "daniel.okafor@solacehealth.example"; phone = "+1 555 0389" }
        @{ key = "jerome";     name = "Jerome Albright";                        email = "jerome.albright@example.com";         phone = "+1 555 0410" }
    )

    $Customers = @{}
    $existingCustomers = @(Invoke-Api -Method GET -Path "/api/customers" -Token $OwnerToken -OrganizationId $OrganizationId)
    foreach ($c in $customerDefs) {
        $match = $existingCustomers | Where-Object { $_.email -eq $c.email } | Select-Object -First 1
        if ($match) {
            $Customers[$c.key] = $match.id
            Write-Host "  = $($c.name) (already exists, reusing)"
            continue
        }
        $apiResult = Invoke-Api -Method POST -Path "/api/customers" -Token $OwnerToken -OrganizationId $OrganizationId -Body @{
            name        = $c.name
            email       = $c.email
            phoneNumber = $c.phone
        }
        $Customers[$c.key] = $apiResult.id
        $Created.customers.Add(@{ id = $apiResult.id; name = $c.name })
        Write-Host "  + $($c.name) <$($c.email)>"
    }

    # ------------------------------------------------------------------------
    # 7. Invite two customers to the portal
    # ------------------------------------------------------------------------
    Section "Inviting customers to the portal"

    $portalCustomers = @("nimbus", "vertex")
    foreach ($key in $portalCustomers) {
        $c = $customerDefs | Where-Object { $_.key -eq $key }
        $inv = Invoke-Api -Method POST -Path "/api/customer-invitations" -Token $OwnerToken -OrganizationId $OrganizationId -Body @{
            customerId = $Customers[$key]
        }
        # Controller doesn't echo the invitation id back, so fetch it isn't
        # possible here - we track by email only; revoke-by-id isn't
        # available for these two without it, so on rollback these two will
        # show as "accepted or unrevoked, check manually" if not yet accepted.
        $Created.customerInvitations.Add(@{ id = $null; email = $c.email; accepted = $false })
        Write-Host "  Invited $($c.name) <$($c.email)>"
    }

    Write-Host ""
    Write-Host "Two more invitation emails arrive at $InboxEmail (tags lumenpeak-cust-nimbus / lumenpeak-cust-vertex):" -ForegroundColor Yellow
    foreach ($key in $portalCustomers) {
        $c = $customerDefs | Where-Object { $_.key -eq $key }
        Write-Host "  - $($c.email.Split('@')[0])" -ForegroundColor Yellow
    }

    foreach ($key in $portalCustomers) {
        $c = $customerDefs | Where-Object { $_.key -eq $key }
        $token = Read-InviteToken "Link or token for $($c.name) <$($c.email)>"
        Invoke-Api -Method POST -Path "/api/customer-invitations/accept" -Body @{
            token    = $token
            password = $SharedPassword
        } | Out-Null
        ($Created.customerInvitations | Where-Object { $_.email -eq $c.email }).accepted = $true
        Write-Host "  Portal access ready for $($c.name) - $($c.email) / $SharedPassword" -ForegroundColor Green
    }

    # ------------------------------------------------------------------------
    # 8. Tickets - NOT reversible once created, see header notes
    # ------------------------------------------------------------------------
    Section "Creating tickets"

    $ticketDefs = @(
        @{ customer="nimbus";     team="Platform Support";   category="Bug Report";             title="Dashboard charts fail to load after latest update"; description="Blank panel where the revenue chart should be, console shows a 500.";     priority="High";     target="Open" }
        @{ customer="vertex";     team="Integrations Team";  category="API / Integration Issue"; title="Webhook retries are duplicating order events";       description="Same order.created event delivered 3-4 times within a minute.";         priority="High";     target="InProgress"; agent="agent1" }
        @{ customer="clearwater"; team="Platform Support";   category="Bug Report";             title="Export button does nothing on Safari";                description="Works on Chrome; no network request fires on Safari 17.";               priority="Medium";   target="InProgress"; agent="agent2" }
        @{ customer="pinecrest";  team="Customer Success";   category="Billing Question";        title="Annual invoice has last year's seat count";          description="Invoiced for 12 seats, actual seat count is 18.";                       priority="Medium";   target="WaitingForCustomer"; agent="agent2" }
        @{ customer="solace";     team="Customer Success";   category="Onboarding Help";          title="Need help migrating 200 existing contacts";          description="Wants a CSV import walkthrough for their existing contact list.";        priority="Low";      target="New" }
        @{ customer="jerome";     team="Customer Success";   category="Billing Question";        title="Can I switch from monthly to annual billing?";       description="Wants pricing difference explained before switching.";                   priority="Low";      target="Resolved"; agent="agent1" }
        @{ customer="nimbus";     team="Platform Support";   category="Performance Issue";       title="Reports take 30+ seconds to generate";                description="Started after last week's update, was under 5 seconds before.";          priority="High";     target="Closed"; agent="agent1" }
        @{ customer="vertex";     team="Integrations Team";  category="API / Integration Issue"; title="API key rotation breaks active webhook";              description="Rotating the key immediately 401s the existing webhook subscription.";    priority="Critical"; target="Open" }
        @{ customer="clearwater"; team="Integrations Team";  category="API / Integration Issue"; title="Need sandbox environment for testing";                description="Wants an isolated environment before going live with the integration.";  priority="Medium";   target="InProgress"; agent="agent2" }
        @{ customer="pinecrest";  team="Platform Support";   category="Bug Report";             title="Typo in password reset email subject";                description="Subject reads 'Reset Your Pasword'.";                                    priority="Low";      target="New" }
        @{ customer="solace";     team="Platform Support";   category="Feature Request";          title="Request: audit log export to CSV";                   description="Needs it for an internal compliance review.";                            priority="Low";      target="New" }
        @{ customer="jerome";     team="Platform Support";   category="Bug Report";             title="Mobile app crashes opening attachment";               description="Crashes on PDF attachments over ~5MB, Android only.";                    priority="Medium";   target="Resolved"; agent="agent2" }
    )

    foreach ($t in $ticketDefs) {
        $ticket = Invoke-Api -Method POST -Path "/api/tickets" -Token $OwnerToken -OrganizationId $OrganizationId -Body @{
            customerId  = $Customers[$t.customer]
            teamId      = $Teams[$t.team]
            categoryId  = $Categories[$t.category]
            title       = $t.title
            description = $t.description
            priority    = $TicketPriority[$t.priority]
        }
        $ticketId = $ticket.id
        $Created.tickets.Add(@{ id = $ticketId; title = $t.title })
        Write-Host "  + [$($t.target)] $($t.title)"

        if ($t.target -eq "New") { continue }

        Invoke-Api -Method POST -Path "/api/tickets/$ticketId/open" -Token $OwnerToken -OrganizationId $OrganizationId | Out-Null
        if ($t.target -eq "Open") { continue }

        $agentUserId = $Users[$t.agent].userId
        Invoke-Api -Method POST -Path "/api/tickets/$ticketId/assign" -Token $OwnerToken -OrganizationId $OrganizationId -Body @{
            agentId = $agentUserId
        } | Out-Null

        $agentToken = $Users[$t.agent].token
        Invoke-Api -Method POST -Path "/api/tickets/$ticketId/start" -Token $agentToken -OrganizationId $OrganizationId | Out-Null
        if ($t.target -eq "InProgress") { continue }

        if ($t.target -eq "WaitingForCustomer") {
            Invoke-Api -Method POST -Path "/api/tickets/$ticketId/wait-for-customer" -Token $agentToken -OrganizationId $OrganizationId | Out-Null
            continue
        }

        Invoke-Api -Method POST -Path "/api/tickets/$ticketId/resolve" -Token $agentToken -OrganizationId $OrganizationId | Out-Null
        if ($t.target -eq "Resolved") { continue }

        Invoke-Api -Method POST -Path "/api/tickets/$ticketId/close" -Token $OwnerToken -OrganizationId $OrganizationId | Out-Null
    }

    # ------------------------------------------------------------------------
    # 9. Comments
    # ------------------------------------------------------------------------
    Section "Adding comments"

    $webhookTicketId = $Created.tickets[1].id   # Vertex webhook duplicates
    Invoke-Api -Method POST -Path "/api/tickets/$webhookTicketId/comments" -Token $Users["agent1"].token -OrganizationId $OrganizationId -Body @{
        content = "Reproduced it - looks like our retry logic isn't checking for an existing delivery ID. Working on a fix."
    } | Out-Null
    Invoke-Api -Method POST -Path "/api/tickets/$webhookTicketId/comments" -Token $OwnerToken -OrganizationId $OrganizationId -Body @{
        content = "This is affecting their order sync, let's treat it as priority."
    } | Out-Null
    Write-Host "  + 2 comments on '$($ticketDefs[1].title)'"

    $billingTicketId = $Created.tickets[5].id   # Jerome billing switch
    Invoke-Api -Method POST -Path "/api/tickets/$billingTicketId/comments" -Token $Users["agent1"].token -OrganizationId $OrganizationId -Body @{
        content = "Explained the annual discount and sent the upgrade link."
    } | Out-Null
    Write-Host "  + 1 comment on '$($ticketDefs[5].title)'"

    # ------------------------------------------------------------------------
    # Done
    # ------------------------------------------------------------------------
    Section "Done - nothing failed, nothing to roll back"

    Write-Host "Organization : $OrgName ($OrganizationId)"
    Write-Host ""
    Write-Host "Logins (all share the password $SharedPassword):"
    Write-Host ("  {0,-10} {1,-16} {2}" -f "Owner", $owner.name, $owner.email)
    foreach ($p in $staff) {
        $roleName = if ($p.role -eq $OrganizationRole.Manager) { "Manager" } else { "Agent" }
        Write-Host ("  {0,-10} {1,-16} {2}  (password: {3})" -f $roleName, $p.name, $p.email, $Users[$p.key].password)
    }
    foreach ($key in $portalCustomers) {
        $c = $customerDefs | Where-Object { $_.key -eq $key }
        Write-Host ("  {0,-10} {1,-16} {2}" -f "Customer", $c.name, $c.email)
    }
    Write-Host ""
    Write-Host "6 categories, 3 teams, 6 customers, 12 tickets across every status created."
}
catch {
    Write-ApiError $_ "seed halted"
    Invoke-Rollback -OwnerToken $OwnerToken -OrganizationId $OrganizationId
    Write-Host ""
    Write-Host "Seed stopped. See above for exactly what was rolled back and what remains." -ForegroundColor Red
    exit 1
}
