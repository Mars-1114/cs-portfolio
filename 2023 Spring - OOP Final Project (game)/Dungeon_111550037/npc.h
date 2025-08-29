#ifndef _NPC
#define _NPC

#include <iostream>
#include <stdio.h>
#include <stdlib.h>
#include <string>
#include <vector>
#include "entity.h"
#include "room.h"
#include "dialogue.h"
#include "item.h"

struct sell {
	int id;
	Item item;
	int price;
	bool permanent;
};

class NPC : public Entity {
private:
	vector<sell> shelf;
	vector<Dialogue> dialogue;
public:
	NPC() = default;
	NPC(string name, int id, vector<sell> _sell, vector<Dialogue> script) : Entity(name, id) {
		shelf = _sell;
		dialogue = script;
	}
	virtual void loadCheck(int);
	//get the commodity the npc has
	vector<sell>* getShelf() {
		return &shelf;
	}
	//get the npc's dialogue
	vector<Dialogue> getDialogue() {
		return dialogue;
	}
};

vector<NPC> npcSummon();

#endif