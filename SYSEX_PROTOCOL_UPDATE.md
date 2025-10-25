# SysEx Protocol Update Summary

## Overview
Updated the sysex message protocol across the entire codebase to match the new specification. This document summarizes the changes made.

## Device Struct Changes (40 bytes)

### Previous Layout:
- Byte 0-1: Device ID (14-bit)
- Byte 2: Boolean values
- Byte 3: Number of Instruments
- Byte 4: Number of Sub Instruments
- Byte 5: Instrument Type
- Byte 6: Platform
- Byte 7: Note Min
- Byte 8: Note Max
- Byte 9-10: Firmware Version (14-bit)
- Byte 20-39: Device Name (20 chars)

### New Layout:
- Byte 0-1: Device ID (14-bit) - **UNCHANGED**
- Byte 2: Number of Instruments - **MOVED** from byte 3
- Byte 3: Number of Sub Instruments - **MOVED** from byte 4
- Byte 4: Instrument Type - **MOVED** from byte 5
- Byte 5: Platform - **MOVED** from byte 6
- Byte 6: Note Min - **MOVED** from byte 7
- Byte 7: Note Max - **MOVED** from byte 8
- Byte 8-9: Firmware Version (14-bit) - **MOVED** from bytes 9-10
- Byte 10-11: Boolean Values (14-bit) - **NEW LOCATION** from byte 2
- Byte 12-19: Reserved - **NEW**
- Byte 20-39: Device Name (20 chars) - **UNCHANGED**

### Device Boolean Values (14-bit at bytes 10-11):
- Bit 0 (0x0001): Muted - **NEW**
- Bit 1 (0x0002): Omni Mode Enable - **UNCHANGED**
- Bits 2-13: Reserved

## Distributor Struct Changes (24 bytes)

### Previous Layout:
- Byte 0-1: Distributor ID (14-bit)
- Byte 2-4: Channels (16-bit)
- Byte 5-9: Instruments (32-bit)
- Byte 10: Distribution Method
- Byte 11: Boolean values (8-bit)
- Byte 12: Min Note Value
- Byte 13: Max Note Value
- Byte 14: Number of Polyphonic Notes
- Byte 15: Reserved

### New Layout:
- Byte 0-1: Distributor ID (14-bit, 255 max) - **UNCHANGED**
- Byte 2-4: Channels (16-bit, bits 15-16 MSB) - **UNCHANGED**
- Byte 5-9: Instruments (32-bit, bits 29-32 MSB) - **UNCHANGED**
- Byte 10: Distribution Method - **UNCHANGED**
- Byte 11: Min Note Value - **MOVED** from byte 12
- Byte 12: Max Note Value - **MOVED** from byte 13
- Byte 13: Number of Polyphonic Notes - **MOVED** from byte 14
- Byte 14-15: Boolean Values (14-bit) - **MOVED** from byte 11, expanded to 14-bit
- Byte 16-23: Reserved - **NEW**

### Distributor Boolean Values (14-bit at bytes 14-15):
- Bit 0 (0x0001): Muted
- Bit 1 (0x0002): Polyphonic Enable
- Bit 2 (0x0004): Note Overwrite
- Bit 3 (0x0008): Damper Enable
- Bit 4 (0x0010): Vibrato Enable - **NEW**
- Bits 5-13: Reserved

## Distribution Methods (unchanged):
- 0: StraightThrough
- 1: RoundRobin
- 2: RoundRobinBalance
- 3: Ascending
- 4: Descending
- 5: Stack

## Files Updated:

### Core Protocol Files:
1. **MMM_Device\Device.cs**
   - Updated `SetDeviceConstruct()` to read from new byte positions
   - Updated `GetDeviceConstruct()` to write to new byte positions
   - Changed boolean handling from byte to 14-bit int (bytes 10-11)
   - Added support for Muted flag in Device boolean values
   - Added reserved bytes 12-19

2. **MMM_Device\Distributor.cs**
   - Updated `ToSerial()` to write to new byte positions
   - Updated `SetDistributor()` to read from new byte positions
   - Changed boolean handling from byte to 14-bit int (bytes 14-15)
   - Added Vibrato property and flag (bit 4)
   - Reordered: MinNote (11), MaxNote (12), NumPolyphonicNotes (13)
   - Added reserved bytes 16-23

### UI/ViewModel Files:
3. **AvaloniaGUI\ViewModels\DistributorManagerViewModel.cs**
   - Added `_vibrato` observable property
   - Updated property change handler to load Vibrato state
   - Updated `Update()` method to include Vibrato
   - Updated `Add()` method to include Vibrato

4. **AvaloniaGUI\Views\DistributorManagerView.axaml**
   - Added "Vibrato Enable" checkbox to UI

### Parser/Handler Files:
5. **MMM_Core\MMM_Parser.cs**
   - No changes required (uses Device and Distributor classes)

## Breaking Changes:
?? **IMPORTANT**: This is a breaking change that requires firmware updates on all devices!

### Compatibility Notes:
- Old firmware will send data in the old format
- New desktop application expects data in the new format
- **Mismatch will cause incorrect parsing of device/distributor configurations**

### Migration Path:
1. Update all device firmware to match new protocol
2. Test with a single device before deploying to all devices
3. Verify all boolean flags are being set/read correctly
4. Test new Vibrato feature

## Testing Checklist:
- [ ] Device discovery and initialization
- [ ] Device configuration read/write
- [ ] Distributor add/remove/update
- [ ] All boolean flags (Muted, OmniMode, Polyphonic, DamperPedal, NoteOverwrite, Vibrato)
- [ ] Distribution method selection
- [ ] Channel and instrument masks
- [ ] Note range (min/max)
- [ ] Number of polyphonic notes
- [ ] Device name handling
- [ ] Reserved bytes are zeroed out

## Additional Notes:
- Boolean values now use 14-bit encoding spread across 2 bytes (7 bits each)
- Reserved bytes should be set to 0 for future compatibility
- Maximum distributor ID is now explicitly limited to 255 (was 16383)
- Vibrato enable flag is available for future use by firmware
