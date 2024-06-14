\ Copyright (c) 2022-2024 Travis Bemann
\ 
\ Permission is hereby granted, free of charge, to any person obtaining a copy
\ of this software and associated documentation files (the "Software"), to deal
\ in the Software without restriction, including without limitation the rights
\ to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
\ copies of the Software, and to permit persons to whom the Software is
\ furnished to do so, subject to the following conditions:
\ 
\ The above copyright notice and this permission notice shall be included in
\ all copies or substantial portions of the Software.
\ 
\ THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
\ IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
\ FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
\ AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
\ LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
\ OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
\ SOFTWARE.

begin-module sh1122-test

  zscript-oo import
  zscript-double import
  zscript-simple-font-6x8 import
  zscript-text-display import
  zscript-sh1122-text import

  0 1 foreign forth::rng::random random
  
  256 constant my-width
  64 constant my-height
    
  6 constant my-char-width
  8 constant my-char-height
    
  \ SPI device
  0 constant my-device

  \ Pins
  3 constant lcd-din 
  2 constant lcd-clk
  7 constant lcd-dc
  6 constant lcd-rst
  5 constant lcd-cs
    
  global my-simple-font
  global my-display
  
  \ Our colors
  15 constant my-fore-color
  0 constant my-back-color
  
  \ Our maximum speed
  my-char-width s>f 6,0 f* constant my-speed
  
  : init-test ( -- )
    make-simple-font-6x8 my-simple-font!
    my-fore-color my-back-color lcd-din lcd-clk lcd-dc lcd-cs lcd-rst
    my-simple-font@ my-width my-height my-device make-sh1122-text my-display!
    s" Hello, World!" { hello }
    hello [: drop $20 ;] map { hello-blank }
    my-display@ dim@ s>f my-char-height s>f f* swap s>f my-char-width s>f f* { dimy dimx }
    dimx -1,0 d+ random 0 2integral>double f* { x }
    dimy -1,0 d+ random 0 2integral>double f* { y }
    random 0 2integral>double 2,0 pi f* f* { angle }
    my-speed angle cos f* my-speed angle sin f* { dx dy }
    hello >len s>f -0,5 f* my-char-width s>f f* -0,5 my-char-height s>f f* { ox oy }
    x ox dnegate dmax to x
    y oy dnegate dmax to y
    dimx -1,0 d+ ox d+ x dmin to x
    dimy -1,0 d+ oy d+ y dmin to y
    systick-counter { last-systick }
    begin key? not while
      x ox d+ my-char-width s>f f/ round-half-down
      y oy d+ my-char-height s>f f/ round-half-down { rx ry }
      hello rx ry my-display@ string!
      my-display@ update-display
      hello-blank rx ry my-display@ string!
      systick-counter { next-systick }
      next-systick last-systick - s>f 10000,0 f/ { interval }
      x dx interval f* d+ to x
      y dy interval f* d+ to y
      x ox dnegate d< x dimx ox d+ d>= or if dx dnegate to dx x dx interval f* d+ to x then
      y oy dnegate d< y dimy oy d+ d>= or if dy dnegate to dy y dy interval f* d+ to y then
      next-systick to last-systick
    repeat
  ;

end-module
