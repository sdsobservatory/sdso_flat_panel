# sdso_flat_panel
Firmware and ASCOM driver for a flat panel controller using an RP2040 and an N-Channel MOSFET

# Hardware
A Pi Pico (RP2040) microcontroller is controlling an N-Channel MOSFET.
A FQP30N06L is used becuase I hade one on hand but any MOSFET with a Vgs equal to or less than 3.3V will work.

* `GP22` to the gate
* 0VDC to the source
* Negative terminal of light panel to drain
* Positive terminal of light panel to 12VDC

# Firmware
PWM is configured for `GP22`.
ASCII commands are received via USB CDC and parsed.

* Commands are space delimited.
* All commands and responses end with a newline character `0x0a`.
* The firmware responds with `#` for a successful command, otherwise it responds `!`.
* Sending a new line character `0x0a` will respond with a `#` character.

| Commands | Description |
| - | - |
| `on` | Turn on the panel to the last known brightness. |
| `off` | Turn off the panel. The current brightness is remembered. |
| `set <value>` | Set the brightness to `value`. Must be between 0 and 1000, inclusive.|

Note: the brightness is not retained on power cycle.

# ASCOM

A simple ASCOM driver implementing `ICoverCalibratorV1` can turn on and off the panel and set its brightness.
See this repo's releases for an installer.
Be sure to select the correct COM port in the ASCOM setup dialog.