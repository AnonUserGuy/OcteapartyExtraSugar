# OcteapartyExtraSugar
A simple BepinEx mod of Bits & Bops that modifies the sugar cues in Octeaparty to be able to use any amount of sugar cubes instead of just 3. 

I do not formally endorse this being used for custom mixtapes, I think that would be unfair. I mostly just made this to experiment in cue modding.

Demo: https://youtu.be/e3MGPssxpI4

## Installation
- Install [BepInEx 5.x](https://docs.bepinex.dev/articles/user_guide/installation/index.html) in Bits & Bops.
- Download `OcteapartyExtraSugar.dll` from the latest [release](https://github.com/AnonUserGuy/OcteapartyExtraSugar/releases/), and place it in ``<Bits & Bops Installation>/BepinEx/plugins/``.

## Usage
### Configuration
After running Bits & Bops with the latest version of this plugin installed, a configuration file will be generated at `BepinEx\config\OcteapartyExtraSugar.cfg`. Open this file with a text editor to access the following configs:
| Name                   | Type          | Default       | Description   |
| ---------------------- | ------------- | ------------- | ------------- |
| `HowMany`              | int           | `3`           | <p>Default amount sugar cubes to spawn per sugar cue.</p> <p>Affects all sugar cues in default game, and sugar cues without an "amount" set in custom mixtapes.</p> |
| `MutateEventTemplates` | Boolean       | `true`        | <p>Whether or not to add the "Amount" parameter to the Octeaparty sugar event in the editor.</p> <p>Disabling **does not** prevent mixtapes from using the parameter, just toggles whether or not the parameter is visible in the mixtape editor.</p> |

### Custom Mixtapes
For custom mixtapes, Octeaparty sugar cues will have a new "Amount" parameter which allows each sugar cue to be given an arbitrary amount of sugar cubes. Setting this to `-1` will make the cue use the amount defined by the `HowMany` parameter, and otherwise will set the amount of sugar cubes just that cue uses. 

Enable `MutateEventTemplates` to have access to this parameter within the mixtape editor. 


## Building 
### Prequisites
- Bits & Bops v1.5+
- Microsoft .NET SDK v4.7.2+
- Visual Studio 2022 (Optional)

### Steps
1. Clone this repository using ``git clone https://github.com/AnonUserGuy/OcteapartyExtraSugar.git``.
2. Copy ``<Bits & Bops installation>/Bits & Bops_Data/Managed/Assembly-CSharp.dll`` into ``OcteapartyExtraSugar/lib/``.
3. Build
    - Using CLI:
      ```bash
      dotnet restore OcteapartyExtraSugar.sln
      dotnet build OcteapartyExtraSugar.sln
      ```
    - Using Visual Studio 2022:
       - Open OcteapartyExtraSugar.sln with Visual Studio 2022.
       - Set build mode to "release".
       - Build project.
4. Copy ``OcteapartyExtraSugar/bin/Release/net472/BopCustomTextures.dll`` into ``<Bits & Bops Installation>/BepinEx/plugins/``.

<br>
<br>
<br>

(...I just realized "amount" isn't the correct term and I should be using something like "number", but whatever I don't feel like going back to fix it.)
