\ Copyright (c) 2024 Travis Bemann
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

begin-module zscript-task

  zscript-queue import
  
  begin-module zscript-task-internal

    \ Queue is empty exception
    : x-queue-empty ( -- ) ." queue is empty" cr ;

    \ Number of ticks per millisecond
    10 constant ticks-per-ms
    
    \ The task queue
    global tasks

    \ Initialize the task queue
    : init-tasks ( -- )
      make-queue tasks!
    ;

    initializer init-tasks

    \ Dequeue from a queue while raising an exception if empty
    : dequeue ( queue -- ) dequeue averts x-queue-empty ;
    
  end-module> import

  \ Schedule a task, which will begin execution with the specified closure or
  \ saved state
  : schedule { task -- }
    task >type dup xt-type = swap closure-type = or if
      task 1 [:
        nip execute
        tasks@ queue-empty? not if
          0 tasks@ dequeue execute
        else
          0
        then
      ;] bind to task
    then
    task tasks@ enqueue
  ;

  \ Yield the current task and execute the first task in the queue unless no
  \ other tasks are queued, where then execution continues as before
  : yield ( -- )
    tasks@ queue-empty? not if
      [: { current-task }
        tasks@ dequeue { next-task }
        current-task schedule
        0 next-task execute
      ;] save drop
    then
  ;

  \ Terminate the current task
  : terminate ( -- )
    tasks@ queue-empty? not if
      tasks@ dequeue { next-task }
      0 next-task execute
    then
  ;

  \ Fork the current task, with the new task being enqueued for future
  \ execution
  : fork ( -- parent? )
    [: { state }
      false 1 state bind schedule
      true
    ;] save
  ;

  \ Start execution of the first enqueued task; note that this word does not
  \ return
  : start ( -- )
    begin tasks@ queue-empty? not while
      [: { current-task }
        tasks@ dequeue { next-task }
        current-task schedule
        0 next-task execute
      ;] save drop
    repeat
  ;

  \ Wake up a task in a queue
  : wake { queue -- }
    queue queue-empty? not if
      queue dequeue schedule
    then
  ;

  \ Block a task in a queue
  : block { queue -- }
    tasks@ queue-empty? not if
      queue [: { queue state }
        tasks@ dequeue { next-task }
        state queue enqueue
        0 next-task execute
      ;] save 2drop
    then
  ;

  \ Wait for a given delay from a time
  : wait-delay { start-time delay -- }
    begin yield systick-counter start-time - delay >= until
  ;

  \ Delay execution of the current task by a specified number of milliseconds
  : ms ( time -- )
    ticks-per-ms * systick-counter swap wait-delay
  ;
  
end-module
