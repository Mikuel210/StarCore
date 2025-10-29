## Master Checklist 

- [x] Redo SDK
- [x] Redo Server
- [x] Communication
- [x] Open protocols
- [x] Close protocols
- [>] Highlight focused instance
- [x] Test changing CanClose, does flyover close?
- [/] UI system
    - [x] Function: list of UI elements -> UI element data
    - [x] Switch to root!
    - [x] Children as IReadOnlyList, have AddChild API etc.
    - [x] Match to controls on client
    - [x] BUG: Switching instances is buggy w current test protocol
    - [/] More controls
    - [ ] Support children ordering
    - [ ] XML support
    - [ ] Better API
- [ ] Serialization framework
- [ ] Audio system
- [ ] Efficient-ify and highlight instance list
- [ ] Handle client disconnection etc. And server switching
- [?] Default separate thread for Instances
- [ ] https://github.com/wieslawsoltes/Dock/blob/master/docs/quick-start.md


# StarUI

The UI framework for StarCore

## Components

- ContainerElement
  - [x] Panel
  - [ ] Grid
  - [ ] Expander
  - [ ] Alert https://getbootstrap.com/docs/5.3/components/alerts/
  - [ ] Dropdown
    - Option
- TextElement
  - [x] TextLabel
  - [x] Button
  - [ ] Input
    - [ ] Text
    - [ ] Number
    - [ ] DateTime
  - [/] Checkbox
  - [ ] Select (radio)
    - Option
- [ ] ImageLabel
- [ ] ProgressBar
- [ ] Calendar

## [ ] Events

## [ ] Colors

https://getbootstrap.com/docs/5.3/utilities/colors/

- Primary
- Secondary
- Light
- Success
- Warning
- Danger
- Info