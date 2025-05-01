using System.Collections;

namespace Commander.Controllers.Directory;

class DirectoryComparer(int index, bool descending) : IComparer
{
    public int Compare(object? item1, object? item2)
    {
        if (item1 is ParentItem && item2 is not ParentItem) return -1;
        if (item1 is not ParentItem && item2 is ParentItem) return 1;

        if (item1 is DirectoryItem && item2 is FileItem) return -1;
        if (item1 is not DirectoryItem && item2 is DirectoryItem) return 1;

        if (item1 is DirectoryItem dir1 && item2 is DirectoryItem dir2)
            return dir1.Name.CompareTo(dir2.Name);

        if (item1 is FileItem file1 && item2 is FileItem file2)
        {
            return index switch
            {
                0 => file1.Name.CompareTo(file2.Name),
                1 => GetDateTime(file1).CompareTo(GetDateTime(file2)),
                2 => (int)(file1.Size - file2.Size),
                3 => CompareVersion(file1, file2),
                _ => 0
            } * (descending ? -1 : 1);
        }
        else
            return 0;
    }

    static int CompareVersion(FileItem item1, FileItem item2)
        => item1.FileVersion == null
            ? -1
            : item2.FileVersion == null
            ? 1
            : item1.FileVersion!.Major != item2.FileVersion.Major
            ? item1.FileVersion.Major - item2.FileVersion.Major
            : item1.FileVersion.Minor != item2.FileVersion.Minor
            ? item1.FileVersion.Minor - item2.FileVersion.Minor
            : item1.FileVersion.Patch != item2.FileVersion.Patch
            ? item1.FileVersion.Patch - item2.FileVersion.Patch
            : item1.FileVersion.Build - item2.FileVersion.Build;

    static DateTime GetDateTime(FileItem item)
        => item.ExifTime != null
            ? item.ExifTime.Value
            : item.DateTime;
}

