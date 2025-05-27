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
                3 => CompareVersion(file1.FileVersion, file2.FileVersion),
                _ => 0
            } * (descending ? -1 : 1);
        }
        else
            return 0;
    }

    public static int CompareVersion(FileVersion? version1, FileVersion? version2)
        => version1 == null
            ? -1
            : version2 == null
            ? 1
            : version1!.Major != version2.Major
            ? version1.Major - version2.Major
            : version1.Minor != version2.Minor
            ? version1.Minor - version2.Minor
            : version1.Build !=version2.Build
            ? version1.Build - version2.Build
            : version1.Patch - version2.Patch;

    static DateTime GetDateTime(FileItem item)
        => item.ExifTime != null
            ? item.ExifTime.Value
            : item.DateTime;
}

