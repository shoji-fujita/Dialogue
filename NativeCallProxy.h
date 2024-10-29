#import <Foundation/Foundation.h>

@protocol NativeCallsProtocol <NSObject>
- (void)unityNotificationWithMessage:(NSString *)message;
@end

__attribute__((visibility("default")))
@interface FrameworkLibAPI : NSObject
+ (void)registerAPIForNativeCalls:(id<NativeCallsProtocol>)api;
+ (void)sendMessageToSwift:(NSString *)message;
@end
