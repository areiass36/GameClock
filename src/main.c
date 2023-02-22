#include <stdio.h>
#include <gb/gb.h>
#include <stdint.h>

#include "sprites/hi.c"
#include "sprites/ball.c"

uint8_t input;

int main()
{
    DISPLAY_ON;
    SHOW_SPRITES;
    SHOW_BKG;

    while (TRUE)
    {
        input = joypad();

        set_sprite_data(0, 1, hi);
        set_sprite_tile(0, 0);
        move_sprite(0, 84, 88);
        /*

        Load background (?)
        set_bkg_data(0, 1, ball_data);
        set_bkg_tiles(0, 0, 1, 1, ball_map);

        */
        wait_vbl_done();
    }

    return 0;
}