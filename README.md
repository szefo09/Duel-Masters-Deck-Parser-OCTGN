# Duel Masters Deck Parser for OCTGN

A simple plugin for parsing Duel Masters decks with **multiline support**.  
The parser matches cards by partial names and always prioritizes **TCG cards** to avoid adding OCG proxies.  

This plugin is based on the example provided in the [OCTGN Source Code](https://github.com/octgn/OCTGN/tree/master/octgnFX/Octgn.DeckBuilderPluginExample).

<img width="328" height="480" alt="{EE4A1CA9-4779-4C06-BBD7-D4DF84E580E3}" src="https://github.com/user-attachments/assets/805ce75e-6366-4428-8390-3f57fb0e1d67" /> <img width="328" height="480" alt="{A5446BAC-2685-45B8-A6F7-D29FD4D188BA}" src="https://github.com/user-attachments/assets/5fe599a9-0db1-48ec-b8e5-b5bd8f57a3a5" />

---

## Features
- Partial name matching for cards
- Multiline deck support
- Prioritizes TCG cards over OCG proxies
- Creates a list from an already loaded deck
---

## Installation
![GitHub release](https://img.shields.io/github/v/release/szefo09/Duel-Masters-Deck-Parser-OCTGN?label=Latest%20Release)
**When installing Duel Masters system 1.1.1.61 or newer, the plugin will autoinstall!**

## Manual Installaion
1. Go to the [Releases page](https://github.com/szefo09/Duel-Masters-Deck-Parser-OCTGN/releases) and download `Octgn.DuelMastersDeckParser.dll`.
2. Navigate to your OCTGN Data Directory, e.g., `OCTGN\Data\Plugins`.
3. Create a folder called `DuelMastersDeckParser`.
4. Copy `Octgn.DuelMastersDeckParser.dll` into the folder.
5. Launch OCTGN and go to **Deck Editor → Plugins** to find the Duel Masters Deck Parser.


---
## Known Issues

When editing existing deck with cards from different reprints other than TCG (Different artworks), they will be normalized to TCG when applying changes.

---

## Support / Donations
*I'm a freelance developer doing this in my free time. If you feel that what I'm doing it worthwhile for the community and deserves a coffee, consider donating to [my Paypal](<https://www.paypal.com/paypalme/szefo09>)*
