using Nuke.Common;

class Release : NukeBuild
{
    Target Sign => _ => _.Executes(() =>
    {
    });
    Target ChangeLog => _ => _.Executes(() =>
    {
    });
    Target CreateRelease => _ => _.Executes(() =>
    {
    });
    Target Push => _ => _.Executes(() =>
    {
    });
}
