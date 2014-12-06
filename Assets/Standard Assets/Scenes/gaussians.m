c = 10;
s = [1,1.25,1.5,1.8,2.4];
m = 8;
vals = meshgrid(-m-0.5:1:m+0.5, s);
f = zeros(size(vals));

figure; hold;

for i = 1:numel(s)
   sigma = s(i);
   x = vals(i,:);
   f(i,:) = (c/(sigma*sqrt(2*pi))*exp(-0.5*((x/sigma).^2)));
end

plot(f');
axis equal;