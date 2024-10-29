#import "NativeCallProxy.h"

static id<NativeCallsProtocol> apiInstance = nil;

@implementation FrameworkLibAPI

+ (void)registerAPIForNativeCalls:(id<NativeCallsProtocol>)api {
    apiInstance = api;
}

+ (void)sendMessageToSwift:(NSString *)message {
    [apiInstance unityNotificationWithMessage:message];
}

@end

extern "C" {
    void sendMessageToSwift(const char *message) {
        NSString *messageString = [NSString stringWithUTF8String:message];
        [FrameworkLibAPI sendMessageToSwift:messageString];
    }
}
