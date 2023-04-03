cmake_minimum_required(VERSION 3.12)
include(${PICO_SDK_PATH}/pico_sdk_init.cmake)

project(SdsoFlatPanel C CXX ASM)
include(PicoSDKConfig.cmake)
pico_sdk_init()

set(CMAKE_C_STANDARD 11)
set(CMAKE_CXX_STANDARD 17)

add_executable(sdso_flat_panel
    flatpanel.c)

target_include_directories(sdso_flat_panel PRIVATE)

target_link_libraries(sdso_flat_panel pico_stdlib hardware_pwm)

pico_enable_stdio_usb(sdso_flat_panel 1)
pico_enable_stdio_uart(sdso_flat_panel 0)

pico_add_extra_outputs(sdso_flat_panel)
