import Foundation

@objc class SwiftBridge: NSObject, NativeCallsProtocol {
    func unityNotificationWithMessage(_ message: String) {
        print("Unityから受信したメッセージ: \(message)")
        // Swiftでの処理
    }
}

// APIの初期設定コード
func registerSwiftBridge() {
    FrameworkLibAPI.registerAPIForNativeCalls(SwiftBridge())
}
