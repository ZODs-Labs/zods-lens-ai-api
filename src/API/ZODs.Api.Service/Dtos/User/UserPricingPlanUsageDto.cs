namespace ZODs.Api.Service.Dtos.User;

public sealed class UserPricingPlanUsageDto
{
    public int PersonalSnippetsMax { get; set; }

    public int PersonalSnippetsLeft { get; set; }

    public int? PersonalSnippetPrefixesMax { get; set; }

    public int? PersonalSnippetPrefixesLeft { get; set; }

    public int? WorkspacesMax { get; set; }

    public int? WorkspacesLeft { get; set; }

    public int? WorkspaceSnippetsMax { get; set; }

    public int? AILensesMax { get; set; }

    public int? AILensesLeft { get; set; }

    public Dictionary<string, int> WorkspaceSnippetsLeft { get; set; } = new();

    public int? WorkspaceSnippetPrefixesMax { get; set; }

    public Dictionary<string, int> WorkspaceSnippetPrefixesLeft { get; set; }

    public int? WorkspaceInvitesMax { get; set; }

    public Dictionary<string, int> WorkspaceInvitesLeft { get; set; }

    public UserPricingPlanUsageDto(
        int personalSnippetsMax,
        int personalSnippetsLeft,
        int? personalSnippetPrefixesMax,
        int? personalSnippetPrefixesLeft,
        int? workspacesMax,
        int? workspacesLeft,
        int? workspaceSnippetsMax,
        int? aiLensesMax,
        int? aiLensesLeft,
        Dictionary<string, int>? workspaceSnippetsLeft,
        int? workspaceSnippetPrefixesMax,
        Dictionary<string, int>? workspaceSnippetPrefixesLeft,
        int? workspaceInvitesMax,
        Dictionary<string, int>? workspaceInvitesLeft)
    {
        PersonalSnippetsMax = personalSnippetsMax;
        PersonalSnippetsLeft = personalSnippetsLeft;
        PersonalSnippetPrefixesMax = personalSnippetPrefixesMax;
        PersonalSnippetPrefixesLeft = personalSnippetPrefixesLeft;
        WorkspacesMax = workspacesMax;
        WorkspacesLeft = workspacesLeft;
        AILensesMax = aiLensesMax;
        AILensesLeft = aiLensesLeft;
        WorkspaceSnippetsMax = workspaceSnippetsMax;
        WorkspaceSnippetsLeft = workspaceSnippetsLeft ?? new();
        WorkspaceSnippetPrefixesMax = workspaceSnippetPrefixesMax;
        WorkspaceSnippetPrefixesLeft = workspaceSnippetPrefixesLeft ?? new();
        WorkspaceInvitesMax = workspaceInvitesMax;
        WorkspaceInvitesLeft = workspaceInvitesLeft ?? new();
    }
}