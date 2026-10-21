# GitHub Release Manual

This guide explains how to create and publish a new versioned **GitHub Release** for **LnzLive**, and how the automated CI/CD pipeline ([`.github/workflows/ci-build.yml`](../.github/workflows/ci-build.yml)) builds and attaches the Windows Desktop release archive (`LnzLive-Windows-x64.zip`).

---

## 1. How Automated Releases Work

The repository's GitHub Actions workflow ([`.github/workflows/ci-build.yml`](../.github/workflows/ci-build.yml)) is configured to trigger automatically whenever a version tag matching `v*` (e.g., `v1.0.0`, `v1.1.0`) or `[0-9]+.[0-9]+*` (e.g., `1.0.0`, `1.1`) is pushed to GitHub:

```yaml
on:
  push:
    branches:
      - main
    tags:
      - "v*"
      - "[0-9]+.[0-9]+*"
```

When a matching tag is pushed:
1. **Provision & Build**: GitHub Actions sets up `.NET 10`, installs **FGVM** (*Friendly Godot Version Manager*), provisions the pinned Godot `.NET` editor and export templates defined in [`src/.fgvm-version`](../src/.fgvm-version), compiles [`src/PetzRendering.csproj`](../src/PetzRendering.csproj), and exports the `Windows Desktop` preset into `build/windows/`.
2. **Upload Workflow Artifact**: Uploads the unpacked contents of `build/windows/` (`PetzRendering.exe`, `PetzRendering.pck`, and `data_PetzRendering_windows_x86_64/`) via `actions/upload-artifact@v4` (which automatically wraps them into a single `LnzLive-Windows-x64.zip` workflow artifact without nesting a second `.zip` inside).
3. **Package & Publish GitHub Release**: Because the run was triggered by a tag (`refs/tags/*`), the workflow compresses `build/windows/*` into `build/LnzLive-Windows-x64.zip` and runs [`softprops/action-gh-release@v2`](https://github.com/softprops/action-gh-release) to:
   - Create (or update) the **GitHub Release** for that tag.
   - Automatically generate release notes from commits and pull requests since the previous tag.
   - Attach `LnzLive-Windows-x64.zip` directly to the Release page as a permanent, publicly downloadable asset.

---

## 2. Pre-Release Checklist

Before tagging a release:

1. **Verify `main` is green**: Ensure all changes intended for the release are merged into `main` and the latest **CI/CD Build & Release** workflow run on `main` has succeeded.
2. **Update Project Version (Optional)**:
   - Open [`src/project.godot`](../src/project.godot) and update `config/version` under `[application]`:
     ```ini
     [application]

     config/name="PetzRendering"
     config/version="1.1.0"
     ```
   - Commit and push the version bump to `main`:
     ```powershell
     git add src/project.godot
     git commit -m "chore: bump version to 1.1.0"
     git push origin main
     ```

---

## 3. Method A: Create a Release via Git CLI (Recommended)

Pushing a Git tag from your terminal is the fastest way to publish a release. GitHub Actions will handle creating the GitHub Release page, generating release notes, and uploading the binary zip archive automatically.

1. **Make sure your local `main` branch is up to date**:
   ```powershell
   git checkout main
   git pull origin main
   ```

2. **Create an annotated version tag** (prefer `vX.Y.Z` semantic versioning, e.g., `v1.0.0`):
   ```powershell
   git tag -a v1.0.0 -m "Release v1.0.0"
   ```

3. **Push the tag to GitHub**:
   ```powershell
   git push origin v1.0.0
   ```

4. **Monitor the Release Workflow**:
   - Open your repository on GitHub and go to the **Actions** tab.
   - You will see a **CI/CD Build & Release** workflow run triggered by your `v1.0.0` tag.
   - Once the job completes (~3–5 minutes with a warm FGVM cache), navigate to the repository's **Releases** section (right sidebar on the main repo page) to view your published release and download `LnzLive-Windows-x64.zip`.
   - You can click **Edit** (pencil icon) on the release at any time to customize the release title or add screenshots/highlights to the auto-generated release notes.

---

## 4. Method B: Create a Release via the GitHub Web UI

You can also create the release directly in your browser on GitHub:

1. Navigate to your repository on GitHub (`https://github.com/<owner>/<repo>`).
2. In the right-hand sidebar, click **Releases** (or go to `https://github.com/<owner>/<repo>/releases`).
3. Click **Draft a new release** (or **Create a new release**).
4. Click **Choose a tag**, type a new version tag such as `v1.0.0`, and select **+ Create new tag: v1.0.0 on publish**.
5. Ensure **Target** is set to `main`.
6. Click **Generate release notes** to automatically populate the title and changelog from recent commits/PRs, and edit the description as desired.
7. Click **Publish release**.
8. **Automatic Binary Upload**:
   - Publishing the release creates the `v1.0.0` Git tag on GitHub, which immediately triggers the **CI/CD Build & Release** workflow under the **Actions** tab.
   - Wait for the workflow run to finish; it will automatically attach `LnzLive-Windows-x64.zip` to the release you just published.

---

## 5. Workflow Artifacts vs. GitHub Release Assets

| Feature | Workflow Artifact (`Actions` Tab) | Release Asset (`Releases` Page) |
| :--- | :--- | :--- |
| **When is it built?** | Every push to `main`, pull request, manual trigger, or tag | Only when a version tag (`v*` or `X.Y*`) is pushed/published |
| **Archive Name** | `LnzLive-Windows-x64` (downloaded as `.zip`) | `LnzLive-Windows-x64.zip` |
| **Archive Contents** | `PetzRendering.exe`, `PetzRendering.pck`, `data_PetzRendering_windows_x86_64/` | `PetzRendering.exe`, `PetzRendering.pck`, `data_PetzRendering_windows_x86_64/` |
| **Who can download?** | Logged-in GitHub users only | Anyone (public direct download link, no login required) |
| **Retention Period** | Expires automatically (default 90 days) | Kept permanently alongside the version tag |

---

## 6. Troubleshooting & Re-Tagging

### Permissions Error on `Publish GitHub Release on Tag Push`
The workflow already declares `permissions: contents: write` in [`.github/workflows/ci-build.yml`](../.github/workflows/ci-build.yml). If the release step ever fails with a `403 Resource not accessible by integration` error:
1. Go to your repository on GitHub → **Settings** → **Actions** → **General**.
2. Scroll down to **Workflow permissions**.
3. Select **Read and write permissions** and click **Save**.

### Moving / Re-Running a Tag After a Fix
If you tagged a commit, discovered a last-minute issue, pushed a fix to `main`, and want to re-assign the same version tag (e.g., `v1.0.0`) to the new commit:

```powershell
# 1. Delete the tag locally
git tag -d v1.0.0

# 2. Delete the tag on GitHub
git push origin :refs/tags/v1.0.0

# 3. Recreate the tag on the latest commit and push again
git tag -a v1.0.0 -m "Release v1.0.0"
git push origin v1.0.0
```
*(If a GitHub Release was already created for that tag, delete the old Release draft/entry in the GitHub **Releases** UI before pushing the recreated tag.)*
