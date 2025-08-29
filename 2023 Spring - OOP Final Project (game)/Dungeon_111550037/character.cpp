#include <iostream>
#include <stdio.h>
#include <stdlib.h>
#include <string>
#include <vector>
#include "entity.h"
#include "character.h"
#include "item.h"
#include "room.h"

void Character::takeDamage(int dmg) {
	int netDMG = dmg - getDEF();
	if (netDMG < 0) {
		netDMG = 0;
	}
	else if (netDMG > getCurHP()) {
		netDMG = getCurHP();
	}
	setCurHP(getCurHP()- netDMG);
}

void Character::attackMessage(int, string) {

}