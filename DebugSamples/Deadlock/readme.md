# Deadlock

This sample demonstrates a classic deadlock scenario in multithreaded applications.

A deadlock occurs when two or more threads are permanently blocked, each waiting for resources held by the other(s):

1. **Background Task**: Locks resources in order: B → A
2. **Main Task**: Locks resources in order: A → B

## Testing

1. Run the application
2. Navigate to `/deadlock` endpoint
3. The request will hang indefinitely

## Diagnosing Deadlocks

1. Capture stack traces with `dotnet-stack` tool
2. Look for patterns where threads are blocked on `Monitor.Enter` calls
