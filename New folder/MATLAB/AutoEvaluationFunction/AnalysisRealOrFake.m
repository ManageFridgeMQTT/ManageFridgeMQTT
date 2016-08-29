function [Result, Per1, Per2] = AnalysisRealOrFake(Dir, BagPath, MeanMhistRGBPath ) 
    load(BagPath)
    load(MeanMhistRGBPath)
    I = imread(Dir);
    I = mat2cell(I,size(I,1));
    Mhist_RGB = zeros(size(I,1),256);
    Mhist_HSV = zeros(size(I,1),256);
    YCbCr = zeros(size(I,1),660);
    %% Extracting CF + DH + Mean DH + DCT
    i = 1;
    z = I{i,1};
    R = z(:,:,1);
    G = z(:,:,2);
    B = z(:,:,3);
    z = rgb2hsv(I{i,1});
    H = z(:,:,1);
    S = z(:,:,2);
    V = z(:,:,3);
    z = I{i,1};
    z = rgb2ycbcr(z);
    Y = z(:,:,1);
    Cb = z(:,:,2);
    Cr = z(:,:,3);
    % RGB
    [c1,~] = imhist(R);
    [c2,~] = imhist(G);
    [c3,~] = imhist(B);
    hist = [c1';c2';c3'];
    h1 = mean(hist);        
    Mhist_RGB(i, 1 : 256) = h1 ./ sum(h1);
    % HSV
    [c1,~] = imhist(H);
    [c2,~] = imhist(S);
    [c3,~] = imhist(V);
    hist = [c1';c2';c3'];
    h1 = mean(hist);
    Mhist_HSV(i, 1 : 256) = h1 ./ sum(h1);
    % YCbCr
    [c1,~] = imhist(Y);
    [c2,~] = imhist(Cb);
    [c3,~] = imhist(Cr);
    YCbCr(i, 1 : 220) = c1(17 : 236);
    YCbCr(i, 221 : 440) = c2(17 : 236);
    YCbCr(i, 441 : 660) = c3(17 : 236);
    S = sum(YCbCr,2);
    for i = 1 : size(I,1)
        V(i,1 : 660) = YCbCr(i,:) ./ S(i,1); 
    end
    YCbCr = V;
    YCbCr = YCbCr(1 : size(I,1), 1 : 200);

    %% SVM model & Bagging
    % Bag 3 Mean DH HSV - rbf
    Test = Mhist_HSV;
    Bag3 = Bag8;
    [B3,~] = predict(Bag3, Test); 

    % Bag6 Y Cb Cr + RGB (polynomail)
    Test = [YCbCr Mhist_RGB];
    Bag6 = Bag11;
    [B6,~] = predict(Bag6, Test); 

    % Bag8 Y Cb Cr + HSV (rbf)
    Test = [YCbCr Mhist_HSV];
    Bag8 = Bag13;
    [B81,~] = predict(Bag8, Test); 

    %  Bagging
    Bag = [B3 B81 B6];
    L = zeros(size(Bag,1),1);
    Per = zeros(size(Bag,1),3);
    for z = 1 : size(Bag,1)   
        [~,c0] = find(Bag(z,:) == 0);
        [~,c1] = find(Bag(z,:) == 1);
        if(isempty(c0))
            L(z) = 1;
            Per(z,1) = 1;
            Per(z,2) = 100;
            Per(z,3) = 0;
        else
            if(isempty(c1))
                L(z) = 0;
                Per(z,1) = 0;
                Per(z,2) = 0;
                Per(z,3) = 100;
            else
                if(size(c0,2) > size(c1,2))
                    L(z) = 0;
                    Per(z,1) = 0;     
                else
                    L(z) = 1;
                    Per(z,1) = 1;
                end
                Per(z,2) = size(c1,2) / size(Bag,2) * 100;
                Per(z,3) = size(c0,2) / size(Bag,2) * 100;
            end
        end    
    end
    Per1 = Per(2);
    Per2 = Per(3);
    Result = L;
    
    if Per2 == 100 
        Result = 0;
    else 
        Result = 1;
    end 
end