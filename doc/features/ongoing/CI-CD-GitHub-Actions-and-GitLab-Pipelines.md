# CI/CD Pipelines (GitHub Actions & GitLab CI) with FGVM

## 📖 Overview
Establish automated Continuous Integration and Continuous Delivery (CI/CD) pipelines for **both GitHub Actions** (`.github/workflows/`) and **GitLab CI/CD** (`.gitlab-ci.yml`), leveraging **FGVM** (*Friendly Godot Version Manager*, configured via `src/.fgvm-version`) to automatically provision the pinned Godot 4 (.NET/Mono) editor and export templates, validate the project, and build/package desktop application artifacts on every push, pull/merge request, and version tag.

## 🎯 Goals & Use Cases
- **Goal 1 (Reproducible Automated Builds):** Use `fgvm` and `src/.fgvm-version` (`4.7.1-stable-mono`) so both GitHub Actions and GitLab CI runners automatically install the exact required Godot version and export templates without manual runner image maintenance.
- **Goal 2 (Dual-Platform CI Parity):** Support both GitHub (`svrgn-eu/LnzLiveGodot4`) and self-hosted/cloud GitLab instances with equivalent build, validation, and artifact-publishing stages.
- **Goal 3 (Automated Release Packaging):** Automatically export and upload desktop build artifacts (e.g., Windows `.exe`/`.pck` zip packages) on branch pushes and attach versioned release archives when Git tags (e.g., `1.0`, `v*`) are pushed.
- **Use Case A:** A developer pushes a commit or opens a Pull/Merge Request to `main`; the pipeline validates that the Godot 4 project imports cleanly, shaders/scripts compile (including `.NET` `dotnet build` once C# is active), and desktop exports succeed.
- **Use Case B:** A maintainer pushes a SemVer tag (e.g., `1.1.0`); the pipeline builds the desktop release package and publishes it to GitHub Releases / GitLab Package Registry.

## 👥 Target Audience / User Roles
- **Project Maintainers & Developers:** Rely on automated build verification and release artifact generation across GitHub and GitLab remotes.
- **End Users / Testers:** Download ready-to-run desktop build artifacts directly from CI runs or Releases without needing to install Godot 4 locally.

## ⚙️ Functional Requirements
- [x] **Requirement 1 (FGVM Version Pinning):** Maintain `src/.fgvm-version` in the Godot project root so `fgvm local` and `fgvm export` resolve the exact Godot version (`4.7.1-stable-mono`) and matching export templates in headless CI environments.
- [x] **Requirement 2 (Export Presets Configuration):** Add a version-controlled `src/export_presets.cfg` defining the target desktop export preset(s) (e.g., `Windows Desktop` runnable executable + `.pck` into `build/windows/`) and ensure `src/.gitignore` allows tracking `export_presets.cfg` if required by `fgvm export`.
- [x] **Requirement 3 (GitHub Actions Workflow):** Create `.github/workflows/ci-build.yml` triggered on `push` (`main` and tags) and `pull_request`:
  - Install .NET SDK and `fgvm`.
  - Run `fgvm local` in `src/` to install the Godot version and export templates specified by `src/.fgvm-version` (with caching for `~/.local/share/fgvm` or `%LOCALAPPDATA%/fgvm`).
  - Run `fgvm export` in `src/` to build all configured desktop presets.
  - Upload the exported desktop build as a GitHub Actions workflow artifact (and attach to a GitHub Release on tag pushes).
- [x] **Requirement 4 (GitLab CI/CD Pipeline):** Create `.gitlab-ci.yml` in the repository root with `prepare`, `build`, and `release` stages:
  - Cache the `fgvm` installation/template directory across pipeline runs.
  - Execute `fgvm local` and `fgvm export` inside `src/`.
  - Publish the exported build directory as GitLab job `artifacts` with expiration policies for branch builds and permanent retention for tagged releases.

## 🎨 UI/UX Design & Interactions
- **No In-App UI Changes:** This feature operates at the repository and DevOps infrastructure level. Build status badges and downloadable desktop artifacts will be surfaced in the GitHub Actions and GitLab CI/CD web interfaces.

## 💻 Technical Details
- **FGVM Configuration (`src/.fgvm-version`):**
  - Generated via `fgvm local 4.7.1-stable-mono` in `src/`.
  - Contains `4.7.1-stable-mono`, instructing `fgvm` which Godot editor binary and export templates to download.
- **GitHub Actions (`.github/workflows/ci-build.yml`):**
  - **Runner:** `windows-latest` or `ubuntu-latest` (with .NET 8 SDK installed).
  - **Steps:**
    1. `actions/checkout@v4`
    2. `actions/setup-dotnet@v4`
    3. Install/restore `fgvm` tool and cache FGVM binary/templates directory.
    4. Working directory `src/`: run `fgvm local` followed by `fgvm export`.
    5. `actions/upload-artifact@v4` to publish the compiled desktop application.
- **GitLab CI (`.gitlab-ci.yml`):**
  - **Stages:** `validate`, `build`, `release`.
  - **Variables:** Set `FGVM_PATH` / cache paths inside the workspace or runner cache so downloaded Godot binaries and export templates persist between jobs.
  - **Script (`src/`):**
    ```yaml
    script:
      - cd src
      - fgvm local
      - fgvm export
    ```
- **Repository Ignore Rules (`src/.gitignore`):**
  - Currently `src/.gitignore` ignores `export_presets.cfg`. To allow `fgvm export` to work out-of-the-box in CI runners, remove `export_presets.cfg` from `src/.gitignore` once desktop export presets are configured.

## 🧪 Testing Plan
- **Local FGVM Verification:**
  - Run `fgvm which` and `fgvm export` inside `src/` locally to verify that `.fgvm-version` and `export_presets.cfg` produce a working desktop executable.
- **GitHub Actions Verification:**
  - Push the workflow to a branch/main on `https://github.com/svrgn-eu/LnzLiveGodot4`, confirm the workflow triggers, installs Godot `4.7.1-stable-mono` via `fgvm`, exports the desktop build, and attaches the downloadable artifact.
- **GitLab Pipeline Verification:**
  - Validate `.gitlab-ci.yml` syntax via GitLab CI Lint and run a pipeline on the GitLab runner to confirm `fgvm local` and `fgvm export` succeed and upload job artifacts.

## 🔗 Related Resources
- [`src/.fgvm-version`](../../src/.fgvm-version)
- [`src/project.godot`](../../src/project.godot)
- [`src/.gitignore`](../../src/.gitignore)
- [ADR-0001: Target Native Desktop Application Scope](../adr/0001-desktop-application-scope.md)
