using System.Diagnostics;

using Commander.Controllers.Directory;

namespace Commander.Extensions;

static class FileVersionInfoExtensions
{
    public static FileVersion? MapVersion(this FileVersionInfo? info)
           => info != null
               ? new(info.FileMajorPart, info.FileMinorPart, info.FilePrivatePart, info.FileBuildPart)
               : null;
}
