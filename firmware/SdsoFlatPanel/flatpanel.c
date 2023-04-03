#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "pico/stdlib.h"
#include "hardware/pwm.h"

#define PANEL_PIN			22
#define MAX_BRIGHTNESS		((uint16_t)1000)
#define RX_BUFFER_LENGTH	128
#define TX_BUFFER_LENGTH	128
#define RX_LOOP_DELAY_MS	5
#define MAX_ARGS			16

static uint16_t brightness = 0;

static uint32_t rx(uint8_t* buffer);
static int dispatch(int argc, char* argv[]);

int main() {
	stdio_init_all();
	stdio_set_translate_crlf(&stdio_usb, false);
	stdio_flush();
	
	gpio_init(PICO_DEFAULT_LED_PIN);
	gpio_init(PANEL_PIN);
	
	gpio_set_dir(PICO_DEFAULT_LED_PIN, GPIO_OUT);
	gpio_set_function(PANEL_PIN, GPIO_FUNC_PWM);
	
	// 10 kHz pwm, 0-1000 wrap
	pwm_config config = pwm_get_default_config();
	pwm_config_set_clkdiv_int_frac(&config, 12, 8);
	pwm_config_set_wrap(&config, MAX_BRIGHTNESS);
	pwm_config_set_output_polarity(&config, false, false);
	pwm_init(pwm_gpio_to_slice_num(PANEL_PIN), &config, true);
	
	// status LED on the board
	gpio_put(PICO_DEFAULT_LED_PIN, 1);
	
	uint8_t rx_buffer[RX_BUFFER_LENGTH] = { 0 };
	uint8_t tx_buffer[TX_BUFFER_LENGTH] = { 0 };
	uint32_t read_count = 0;
	char *token;
	char *argv[MAX_ARGS] = { 0 };
	size_t argc = 0;
	char delim[] = " ";
	
	while (true) {
		read_count = rx(rx_buffer);
		if (read_count == 1 && rx_buffer[0] == '\n')
		{
			printf("#\n");
		}
		else if (read_count > 0 && rx_buffer[read_count - 1] == (uint8_t) '\n')
		{
			// replace the \n in the rx_buffer with a null
			rx_buffer[read_count - 1] = 0;
			read_count--;
			
			// tokenize the command
			argc = 0;
			token = strtok((char*) rx_buffer, delim);
			while (token != NULL && argc <= MAX_ARGS)
			{
				argv[argc++] = token;
				token = strtok(NULL, delim);
			}
			argv[argc] = NULL;			
			
			int result = dispatch(argc, argv);
			tx_buffer[0] = result ? '!' : '#';
			tx_buffer[1] = '\n';
			tx_buffer[2] = 0;
			printf("%s", tx_buffer);
		}
		
		memset(rx_buffer, 0, RX_BUFFER_LENGTH);
		sleep_ms(RX_LOOP_DELAY_MS);
	}
}

static uint32_t rx(uint8_t* buffer)
{
	uint32_t buffer_byte_count = 0;
	int c = PICO_ERROR_TIMEOUT;
	while (buffer_byte_count < RX_BUFFER_LENGTH)
	{
		c = getchar_timeout_us(1);
		if (c == PICO_ERROR_TIMEOUT) break;
		buffer[buffer_byte_count++] = (uint8_t) c;
		sleep_ms(RX_LOOP_DELAY_MS);
	}
	
	return buffer_byte_count;
}

static int dispatch(int argc, char* argv[])
{
	if (argc == 0) return 1;
	
	if (strcmp(argv[0], "on") == 0)
	{
		pwm_set_gpio_level(PANEL_PIN, brightness);
		return 0;
	}
	else if (strcmp(argv[0], "off") == 0)
	{
		pwm_set_gpio_level(PANEL_PIN, 0);
		return 0;
	}
	else if (strcmp(argv[0], "set") == 0 && argc == 2)
	{
		int value = strtol(argv[1], NULL, 10);
		if (value < 0)
			brightness = 0;
		else if (value > MAX_BRIGHTNESS)
			brightness = MAX_BRIGHTNESS;
		else
			brightness = (uint16_t) value;
		
		pwm_set_gpio_level(PANEL_PIN, brightness);
		return 0;
	}
	
	return 1;
}