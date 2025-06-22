# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [v1.0.1] - 2024-12-22

### 🐛 Bug Fixes
- **Critical**: Fixed global hotkey registration failure that prevented `Shift+Enter` from working
- **Critical**: Fixed hotkey detection logic for proper key combination matching
- Fixed window handle issue in global hotkey system by adding hidden form
- Fixed modifier key detection (Ctrl, Shift, Alt, Win) for more reliable hotkey capture

### ✨ New Features
- **Interactive Hotkey Setting**: Click textbox and press key combination to set custom hotkeys
- **Real-time Hotkey Capture**: Visual feedback when capturing new hotkey combinations
- **Enhanced Key Display**: User-friendly key name display (e.g., "Ctrl+Enter" instead of key codes)
- **Hotkey Validation**: Automatic validation of hotkey combinations

### 🔧 Improvements
- Enhanced OpenAI API calls with proper User-Agent header for better compatibility
- Improved hotkey persistence and loading in settings
- Better error handling for hotkey conflicts
- More robust modifier key detection across different keyboard layouts

### 📖 Documentation
- Added comprehensive README with installation and usage instructions
- Created detailed project documentation with features and technical specifications
- Added MIT license and contribution guidelines
- Set up GitHub Actions workflow for automated builds

### 🏗️ Technical Changes
- Created `HotkeyCapture` utility class for interactive hotkey management
- Replaced readonly hotkey textboxes with interactive `HotkeyTextBox` controls
- Added hidden form for proper window handle management
- Enhanced global hotkey detection algorithm

---

## [v1.0.0] - 2024-12-22

### 🎉 Initial Release

### ✨ Core Features
- **Global Floating Translation Window**: Press `Shift+Enter` anywhere to open translation interface
- **Smart Translation**: Automatic Chinese ⇄ English translation with language detection
- **One-Click Copy**: Press `Ctrl+Enter` again to copy translated text to clipboard
- **Translation History**: Local SQLite database with full search and pagination support
- **Multiple Translation Scenarios**: Life, Formal, and Technical translation styles
- **System Tray Integration**: Background operation with right-click context menu

### 🔧 Technical Implementation
- **Framework**: .NET 8 WinForms for native Windows experience
- **Database**: SQLite with Microsoft.Data.Sqlite for local data storage
- **Security**: AES encryption for API keys and sensitive configuration
- **API Integration**: OpenAI-compatible REST API support with custom BaseURL
- **Global Hotkeys**: Low-level keyboard hook for system-wide hotkey detection

### 🎨 User Interface
- **Borderless Floating Window**: Clean, minimal design that follows cursor position
- **Auto-sizing**: Dynamic height adjustment based on content
- **History Navigation**: Browse previous translations with arrow buttons
- **Settings Management**: Tabbed interface for comprehensive configuration
- **Visual Feedback**: Loading indicators and status messages

### 📊 Data Management
- **Local Storage**: All translation history stored locally for privacy
- **Search Functionality**: Full-text search across translation history
- **Export/Import**: Easy backup and restore of translation data
- **Pagination**: Efficient handling of large translation history

### 🔒 Security & Privacy
- **Local-First**: No data sent to third parties except for translation API calls
- **Encrypted Storage**: API keys protected with AES encryption
- **Minimal Permissions**: Only requests necessary system permissions
- **Privacy Focused**: Translation history remains private and local

### ⚙️ Configuration Options
- **Custom API Settings**: Support for any OpenAI-compatible service
- **Hotkey Customization**: Configurable global hotkeys for all functions
- **UI Customization**: Adjustable window size, opacity, and appearance
- **Translation Scenarios**: Customizable prompts for different contexts

### 🚀 Performance
- **Fast Startup**: Quick application initialization and hotkey registration
- **Responsive UI**: Smooth animations and immediate feedback
- **Efficient Memory Usage**: Optimized for continuous background operation
- **Reliable Hotkeys**: Robust global hotkey system that works across applications

---

## Version History Summary

| Version | Release Date | Key Features |
|---------|-------------|--------------|
| v1.0.1  | 2024-12-22  | 🐛 Critical hotkey fixes, interactive settings |
| v1.0.0  | 2024-12-22  | 🎉 Initial release with full feature set |

---

## Upcoming Features (Roadmap)

### v1.1.0 (Planned)
- [ ] **Dark Mode Support**: Complete dark theme implementation
- [ ] **Multiple Language Pairs**: Support for more language combinations
- [ ] **Translation Memory**: Smart suggestions based on previous translations
- [ ] **Plugin System**: Extensible architecture for custom translation providers
- [ ] **Batch Translation**: Process multiple texts simultaneously
- [ ] **OCR Integration**: Translate text from images and screenshots

### v1.2.0 (Planned)
- [ ] **Cloud Sync**: Optional cloud backup for settings and history
- [ ] **Team Features**: Shared translation glossaries and terminology
- [ ] **Advanced Statistics**: Detailed usage analytics and insights
- [ ] **Export Formats**: PDF, Word, and other document export options
- [ ] **Voice Input**: Speech-to-text for hands-free translation
- [ ] **Mobile Companion**: Cross-platform synchronization

---

## Contributing

We welcome contributions! Please see our [Contributing Guidelines](../README.md#contributing) for details.

## Support

- 🐛 **Bug Reports**: [GitHub Issues](https://github.com/maxazure/TransInputMethod/issues)
- 💬 **Discussions**: [GitHub Discussions](https://github.com/maxazure/TransInputMethod/discussions)
- 📧 **Email**: maxazure@gmail.com

## License

This project is licensed under the MIT License - see the [LICENSE](../LICENSE) file for details.