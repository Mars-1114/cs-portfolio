#include <stdio.h>
#include <stdlib.h>
#include <iostream>
#include <string.h>
#include "room.h"
#include "entity.h"
#include "player.h"
#include "dialogue.h"

int nextLvlReq(int);
Item* findItem(Item, vector<Item>&);

void Player::showStats() {
	cout << getName() << " Status" << endl << endl;
	cout << "Occupation: " << getOccName() << endl;
	cout << "Weapon: " << mainWeapon->getName() << endl;
	cout << "Armor: " << mainArmor->getName() << endl << endl;
	cout << "Health: " << getCurHP() << "/" << getMaxHP() << endl;
	cout << "ATK: " << getATK() << endl;
	cout << "DEF: " << getDEF() << endl;
	cout << "LV: " << getLvl() << " (" << getExp() << "/" << nextLvlReq(getLvl()) << ")" << endl;
	cout << "Coin: " << coin << "G" << endl << endl;
}

void Player::updateStatus(int _hp, int _atk, int _def, int _coin) {
	setMaxHP(getMaxHP() + _hp);
	setCurHP(getCurHP() + _hp);
	setATK(getATK() + _atk);
	setDEF(getDEF() + _def);
	coin += _coin;
}

void Player::addItem(Item stuff) {
	checkMatchedOcc(stuff);
	if (stuff.getEquippable() == 0) {
		updateStatus(stuff.getHP(), stuff.getATK(), stuff.getDEF(), stuff.getCoin());
	}
	if (stuff.getName() != "coin") {
		Item tempWeapon;
		Item tempArmor;
		if (mainWeapon != NULL) {
			tempWeapon = *mainWeapon;
		}
		if (mainArmor != NULL) {
			tempArmor = *mainArmor;
		}
		backpack.push_back(stuff);
		if (mainWeapon != NULL) {
			mainWeapon = findItem(tempWeapon, backpack);
		}
		if (mainArmor != NULL) {
			mainArmor = findItem(tempArmor, backpack);
		}
	}
}

void Player::removeItem(int id) {
	Item tempWeapon;
	Item tempArmor;
	if (mainWeapon != NULL) {
		tempWeapon = *mainWeapon;
	}
	if (mainArmor != NULL) {
		tempArmor = *mainArmor;
	}
	if (backpack[id].getEquippable() == 0) {
		updateStatus(-backpack[id].getHP(), -backpack[id].getATK(), -backpack[id].getDEF());
	}
	backpack.erase(backpack.begin() + id);
	if (mainWeapon != NULL) {
		mainWeapon = findItem(tempWeapon, backpack);
	}
	if (mainArmor != NULL) {
		mainArmor = findItem(tempArmor, backpack);
	}
}

void Player::setWeapon(Item& weapon) {
	if (mainWeapon != NULL) {
		updateStatus(-mainWeapon->getHP(), -mainWeapon->getATK(), -mainWeapon->getDEF());
	}
	mainWeapon = &weapon;
	updateStatus(mainWeapon->getHP(), mainWeapon->getATK(), mainWeapon->getDEF());
}

void Player::setArmor(Item& armor) {
	if (mainArmor != NULL) {
		updateStatus(-mainArmor->getHP(), -mainArmor->getATK(), -mainArmor->getDEF());
	}
	mainArmor = &armor;
	updateStatus(mainArmor->getHP(), mainArmor->getATK(), mainArmor->getDEF());
}

void occDesc(int id) {
	cout << endl;
	if (id == 0) {
		cout << "The fearless soldier. Not the most outstanding one, yet gives the most balanced play." << endl;
		cout << "You will begin with -" << endl;
		cout << "  HP: 100" << endl;
		cout << "  ATK: 40" << endl;
		cout << "  DEF: 30" << endl;
	}
	else if (id == 1) {
		cout << "The ruthless shooter. They're born with deadly precision, but beware not to get flanked." << endl;
		cout << "You will begin with -" << endl;
		cout << "  HP: 100" << endl;
		cout << "  ATK: 45" << endl;
		cout << "  DEF: 25" << endl;
	}
	else if (id == 2) {
		cout << "The flawless magician. The ultimate power requires an exchange, can you handle this kind of sacrifice?" << endl;
		cout << "You will begin with -" << endl;
		cout << "  HP: 80" << endl;
		cout << "  ATK: 40" << endl;
		cout << "  DEF: 20" << endl << endl;
		cout << "(+ range attack)" << endl;
	}
}

void Player::attackMessage(int dmg, string target) {
	printLine("(You deals " + target + " " + to_string(dmg) + " points)", 1);
}

void Player::gainExp(int _exp) {
	int cmp = getLvl();
	exp += _exp;
	if (exp < nextLvlReq(1)) {
		setLvl(1);
	}
	else if (exp < nextLvlReq(2)) {
		setLvl(2);
	}
	else if (exp < nextLvlReq(3)) {
		setLvl(3);
	}
	else if (exp < nextLvlReq(4)) {
		setLvl(4);
	}
	else {
		exp = nextLvlReq(4);
		setLvl(5);
	}
	if (cmp != getLvl()) {
		int upgrade = (int)(getLvl() * 0.5 * getLvl() * 0.5);
		updateStatus(20 * upgrade, 15 * upgrade, 10 * upgrade);
		if (getLvl() == 5) {
			printLine("(You have reached the max level)");
		}
		else {
			printLine("(Your level has increased to " + to_string(getLvl()) + ")");
		}
	}
}

int nextLvlReq(int _lvl) {
	if (_lvl == 1) {
		return 10;
	}
	else if (_lvl == 2) {
		return 30;
	}
	else if (_lvl == 3) {
		return 60;
	}
	else if (_lvl == 4) {
		return 100;
	}
	else {
		return 100;
	}
}

Item* findItem(Item target, vector<Item>& pool) {
	for (int i = 0; i < (int)pool.size(); i++) {
		if (pool[i].getName() == target.getName()) {
			return &(pool[i]);
		}
	}
	return NULL;
}

void Player::checkMatchedOcc(Item& target) {
	if (occupation + 1 == target.getSpecialized()) {
		if (occupation == 0) {
			target.setATK(100);
		}
		else if (occupation == 1) {
			target.setATK(100);
		}
		else if (occupation == 2) {
			target.setATK(100);
		}
	}
	target.setSpecialized(0);
}