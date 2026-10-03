using System.Collections.Generic;

public static class OrderRules
{
    // Plates are multisets; drink layers are ordered. Small (<=3) orders need no temporary list.
    public static bool Matches(IReadOnlyList<DraggableObjectSO> submitted,
        IReadOnlyList<DraggableObjectSO> expected, bool ordered)
    {
        if (submitted == null || expected == null || submitted.Count == 0 || submitted.Count != expected.Count) return false;
        for (int i = 0; i < submitted.Count; i++)
        {
            if (submitted[i] == null || expected[i] == null) return false;
            if (ordered)
            {
                if (submitted[i] != expected[i]) return false;
                continue;
            }
            int actualCount = 0, expectedCount = 0;
            for (int j = 0; j < submitted.Count; j++)
            {
                if (submitted[j] == submitted[i]) actualCount++;
                if (expected[j] == submitted[i]) expectedCount++;
            }
            if (actualCount != expectedCount) return false;
        }
        return true;
    }
}
