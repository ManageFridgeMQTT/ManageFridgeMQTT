function InstallVLFLib(PATH)
    %%
    % Author: Ung Quang Huy
    % This function installs VLFeat Library (Open Sources).
    % Homepage of VLFeat: http://www.vlfeat.org/
    % Input: PATH is the path of 'vl_setup.m' function in VLFeat Library.
    % File 'vl_setup.m' locates in VLFeat\toolbox\
    % Defaut: PATH = 'VLFeat_Lib\toolbox\vl_setup.m'.
    %%
%     if (nargin == 0)
%         PATH = 'VLFeat_Lib\toolbox\vl_setup.m';
%     end
    if(exist('vl_version') ~= 3)
        run(PATH);
        fprintf('Installation of VLFeat Lib is successful.\n');
    end
    
end