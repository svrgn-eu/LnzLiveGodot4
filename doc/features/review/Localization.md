# Feature: Localization System

## 📖 Overview
The Localization System enables the game to be translated into multiple languages by introducing localization keys for all player-facing text. This ensures the game can support a global audience and makes managing text assets central and scalable.

## 🎯 Goals & Use Cases
- **Goal 1:** Decouple hardcoded text from UI components and game logic.
- **Goal 2:** Provide a scalable way to add new languages (e.g., English, French, Spanish).
- **Use Case A:** A player switches the game language in the Settings menu, and all UI text immediately updates to the selected language.
- **Use Case B:** A developer adds a new UI button and uses a translation key (e.g., `UI_BUTTON_START`) instead of the literal string.

## 👥 Target Audience / User Roles
- **Players:** Can enjoy the game in their native language.
- **Translators:** Can easily edit translation files without touching code.

## ⚙️ Functional Requirements
- [ ] Requirement 1: All UI text (menus, buttons, HUD) must use localization keys.
- [ ] Requirement 2: Dialogue, barks, and item descriptions must be localizable.
- [ ] Requirement 3: The system must support loading Gettext (`.po`/`.pot`) or CSV files provided by Godot's built-in localization features.
- [ ] Requirement 4: The Settings menu must allow players to change the language at runtime.
- [ ] Requirement 5: The system must handle dynamic text (e.g., variables, player names) and pluralization natively.
- [ ] Requirement 6: Specific resources (e.g., fonts, images containing text) must support localization remapping.

## 🎨 UI/UX Design & Interactions
- **Settings Menu:** Add a dropdown for "Language" in the General or Display settings.
- If the language is changed during gameplay, a reload or immediate UI refresh should be triggered.
- **RTL Support:** UI layouts must be built using proper containers (e.g., `HBoxContainer`) to dynamically support Right-To-Left (RTL) languages like Arabic or Hebrew if they are ever needed.

## 💻 Technical Details
- **Godot Implementation:** Use Godot's built-in `TranslationServer`.
- **File Structure:**
  - Create a `locales/` or `data/translations/` directory in the project root.
  - **Format Recommendation (Godot Best Practice):** Use Gettext (`.po`/`.pot`) files. They natively support multi-line strings, context, and complex pluralization, which CSV localization lacks in Godot out-of-the-box.
- **Usage in Code:**
  - *Static Text:* Use `Tr("KEY_NAME")` in C# scripts (inheriting `Node`).
  - *Contexts (Godot Best Practice):* Use translation contexts for ambiguous words, e.g., `Tr("Match", "Game")` vs `Tr("Match", "Fire")`, to provide clarity to translators.
  - *Dynamic Text:* Use string formatting, e.g., `string.Format(Tr("ITEM_COLLECTED"), itemCount)`.
  - *Pluralization:* Use `TrN("SINGULAR_KEY", "PLURAL_KEY", count)` to automatically handle singular vs. plural grammar rules.
  - *Runtime Switching:* Call `TranslationServer.SetLocale("es")` to switch languages. Auto-translating UI nodes will update immediately.
- **Usage in UI:**
  - **Auto Translation (Godot Best Practice):** Nodes like `Label` and `Button` have built-in auto-translation properties. Set their `Text` property directly to the key, and Godot will handle the rest without script intervention.
- **Fonts and Resource Handling:**
  - **Font Fallbacks (Godot Best Practice):** Instead of manually remapping entire font resources per locale, use Godot's Font Fallback system (e.g., adding a CJK font as a fallback) to automatically handle missing glyphs.
  - *Resource Remapping:* Use Godot's locale-based resource remapping to seamlessly swap localized assets (e.g., voiceover audio or language-specific textures).

## 🧪 Testing Plan
- **Pseudolocalization (Godot Best Practice):** Enable Godot's built-in pseudolocalization feature during development (in Project Settings). It replaces characters with accents, stretches strings to simulate long languages, and makes hardcoded (untranslated) text instantly obvious.
- Verify that changing the language in the Settings menu updates all text in the main menu and in-game HUD instantly.
- Test for missing keys (ensure fallback text or key name is displayed).
- Test text expansion using Pseudolocalization to ensure UI layouts (like `VBoxContainer`/`HBoxContainer`) adapt properly and text does not clip or overflow.
