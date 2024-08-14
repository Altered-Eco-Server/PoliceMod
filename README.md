# ECO Prison Mod

## Overview

This ECO mod introduces a system for incarcerating players and impounding vehicles, adding a new layer of gameplay to your server. As a server administrator or police officer in the game, you can arrest players, assign them to cells, and manage their sentences. Players who attempt to escape will face extended sentences and reputation penalties. Vehicles impounded are teleported to the impound where  the player must pay a ticket to retrieve it.

## Features

- **Arrest Players**: Players can be arrested and placed in cells, with a sentence determined by the arresting authority.
- **Impound Vehicles**: Vehicles can be impounded and placed in an impound, with a ticket issued to the offending player.
- **Incarceration Tracking**: The mod tracks incarcerated players, their remaining sentence time, and their cell locations.
- **Escape Attempts**: Players attempting to escape will have their sentence extended and reputation penalized.
- **Sentence Management**: Modify the sentence time for players, either extending or reducing their time in prison.
- **Release Mechanism**: Automatically release players once their sentence is complete, or manually release them through commands.
- **Persistent Data**: Incarceration data is saved and can be restored upon server restart, ensuring that player sentences continue where they left off.

## Commands

- **/sendtocell [player] [cellPos] [hours] [reason]**: Arrests the specified player and places them in the designated cell for the specified number of hours.
- **/giveticket [reason]**: Impounds the vehicle the authority has highlighted and issues a ticket to the offending player.
- **/releasefromjail [player]**: Releases the specified player from prison.
- **/modifysentence [player] [hours]**: Modifies the sentence of the specified player by the given number of hours.
- **/checksentence [player]**: Displays the remaining time of the player's sentence.
- **/trafficrecords**: Displays a record of all traffic tickets issued.
- **/jailrecords**: Displays a record of all arrests.
- **/prisoners [player]**: Display a list of a prisoners and details.
- **/citizenrecord [player]**: Generate a record for a player including slgID, Residency, Birthdate, Age, Reputation, Arrests, Tickets, and Escape attempts. 
  
## Installation

1. Download the latest release of the ECO Prison Mod from the [releases page](#).
2. Place the mod files in your server's `Mods` directory.
3. Restart the server.

## Configuration

The mod allows for the following configuration adjustments:

- **Cell Positions**: Define the positions of the prison cells within the mods configuration file.
- **Impound Position**: Define the positions of the prison cells within the mods configuration file.
