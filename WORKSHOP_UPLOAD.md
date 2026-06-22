# Steam Workshop Upload Guide

Repo prep is done. Upload still happens **in RimWorld** (dev mode → Mods → Upload on Steam). Use **Publisher Plus** so `Source/`, `.git/`, and `DEVELOPMENT.md` are excluded via `_PublisherPlus.xml`.

## One-time setup

1. Subscribe to **Publisher Plus** (or a 1.6-compatible fork) on Steam Workshop.
2. Enable **dev mode** in RimWorld options.
3. Ensure both mod folders are in `RimWorld/Mods/` (symlinks are fine).
4. Enable **Steam Guard** (needed to set visibility to Private on the Workshop website).

## Before each upload

```bash
# Witcher Base
cd /path/to/witcher/Source && ./build.sh

# Witcher Potions
cd /path/to/witcher-potions/Source && ./build.sh
```

Enable **Publisher Plus** in the mod list alongside the mod you are uploading.

---

## 1. Upload Witcher Base (first)

1. Launch RimWorld via Steam.
2. Main menu → **Mods** → select **Witcher Base**.
3. Click **Upload on Steam** (bottom; requires dev mode).
4. Confirm name, description, and preview image.
5. On success, RimWorld creates `About/PublishedFileId.txt` in the mod folder.
6. **Commit that file to git** — required for all future updates.
7. Open the Workshop item in a browser → **Owner controls** → **Change visibility** → **Private**.

Copy the numeric ID from `About/PublishedFileId.txt` (e.g. `1234567890`).

---

## 2. Wire Potions → Base Workshop ID

Edit `witcher-potions/About/About.xml` and add `steamWorkshopUrl` inside the existing `witcher.base` dependency:

```xml
<modDependencies>
  <li>
    <packageId>witcher.base</packageId>
    <displayName>Witcher Base</displayName>
    <steamWorkshopUrl>steam://url/CommunityFilePage/YOUR_BASE_ID_HERE</steamWorkshopUrl>
  </li>
</modDependencies>
```

Replace `YOUR_BASE_ID_HERE` with the number from Base's `PublishedFileId.txt`.

---

## 3. Upload Witcher Potions (second)

1. Rebuild Potions DLL (`./Source/build.sh`).
2. RimWorld → **Mods** → select **Witcher Potions** → **Upload on Steam**.
3. Commit `About/PublishedFileId.txt` to the potions repo.
4. Set visibility to **Private** on the Steam Workshop page.

---

## Testing on another device

| Visibility | Who can install |
|------------|-----------------|
| **Private** | Same Steam account only |
| **Unlisted** | Anyone with the direct Workshop URL |

On the other machine: subscribe to both mods → enable **Biotech**, **Witcher Base**, **Witcher Potions** (Base before Potions).

## Updating later

1. Rebuild DLL.
2. Mods → select mod → **Upload on Steam** again (uses existing `PublishedFileId.txt`).
