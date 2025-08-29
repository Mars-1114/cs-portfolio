#include <iostream>
#include <stdio.h>
#include <stdlib.h>
#include <string>
#include <vector>
#include "entity.h"
#include "item.h"
#include "room.h"
#include "Dungeon.h"

extern Dungeon dungeon;

vector<Item> itemSummon() {
	vector<Item> list;
	list.push_back(Item("coin", 1, 0, 0, 0, 10));
	list.push_back(Item("coin", 3, 0, 0, 0, 10));
	list.push_back(Item("Swift Boost", 5, 0, 20, 0, 0));
	list.push_back(Item("coin", 8, 0, 0, 0, 15));
	list.push_back(Item("Magic Barrier", 10, 10, -20, 50, 0, 2));
	list.push_back(Item("coin", 13, 0, 0, 0, 10));
	list.push_back(Item("Mysterious Potion", 15, 30, 0, -10, 0));
	list.push_back(Item("Old Toolbox", 15, 0, 0, 0, 0));
	return list;
}

void Item::loadCheck(int _id) {
	extern vector<Item*> itemLoad;
	if (getRoomID() == _id) {
		itemLoad.push_back(this);
	}
}

void Item::showStats() {
	cout << endl;
	if (addCoin != 0) {
		cout << addCoin << "G" << endl;
	}
	else {
		if (equippable == 3) {
			cout << "Eating this will give you:" << endl;
			cout << "+ 30 health" << endl;
		}
		else {
			if (equippable != 0) {
				cout << "Equipping this will give you:" << endl;
			}
			else {
				if (addHP != 0 || addATK != 0 || addDEF != 0) {
					cout << "Bearing this will give you:" << endl;
				}
			}
			if (addHP != 0) {
				if (addHP > 0) {
					cout << "+";
				}
				cout << addHP << " health boost" << endl;
			}
			if (addATK != 0) {
				if (addATK > 0) {
					cout << "+";
				}
				cout << addATK << " strength" << endl;
			}
			if (addDEF != 0) {
				if (addDEF > 0) {
					cout << "+";
				}
				cout << addDEF << " resistence" << endl;
			}
			if (specialized == dungeon.player.getOcc() + 1) {
				cout << endl << "(This might get more powerful on you hand)" << endl;
			}
		}
	}
}