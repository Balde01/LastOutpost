using UnityEngine;

public static class ValidatorReferences
{
    public static bool Validate(Object owner, params (Object reference, string name)[] references)
    {
        string ownerName = owner != null ? owner.GetType().Name : "UnknownOwner";
        bool isValid = true;

        if (references == null || references.Length == 0)
        {
            Debug.LogError($"[{ownerName}] No references were provided for validation.", owner);
            return false;
        }

        foreach (var (reference, name) in references)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                Debug.LogError($"[{ownerName}] A reference name is null or empty.", owner);
                isValid = false;
                continue;
            }

            if (reference == null)
            {
                Debug.LogError($"[{ownerName}] Reference '{name}' is not assigned.", owner);
                isValid = false;
            }
        }

        return isValid;
    }

    public static bool ValidateArray(Object owner, Object[] references, string arrayName)
    {
        string ownerName = owner != null ? owner.GetType().Name : "UnknownOwner";

        if (string.IsNullOrWhiteSpace(arrayName))
        {
            Debug.LogError($"[{ownerName}] Array name is null or empty.", owner);
            return false;
        }

        if (references == null)
        {
            Debug.LogError($"[{ownerName}] Reference array '{arrayName}' is null.", owner);
            return false;
        }

        if (references.Length == 0)
        {
            Debug.LogError($"[{ownerName}] Reference array '{arrayName}' is empty.", owner);
            return false;
        }

        bool isValid = true;

        for (int i = 0; i < references.Length; i++)
        {
            if (references[i] == null)
            {
                Debug.LogError($"[{ownerName}] Reference '{arrayName}[{i}]' is not assigned.", owner);
                isValid = false;
            }
        }

        return isValid;
    }
}