#include <iostream>
#include <stdio.h>
#include <stdlib.h>
#include <string>
#include "entity.h"
#include "room.h"
#include "npc.h"
#include "dialogue.h"

//HOW THE DIALOGUE CATEGORIZED//
/*
<Title>
	00: General menu
	01: Trade menu default
	02: Chat menu default
<List>
	05-09: Chat topic
<Trade>
	10-19: Selling item description
	20: Deal
	21: Not Deal
	22: Not the matching occupation
	23: Matching occupation
<Chat>
	30-79: Dialogue, each get 10 slots
	80-99: Placeholder
	100: Leave message
*/

vector<NPC> npcSummon() {
	vector<NPC> tempNPC;
	vector<sell> tempShelf;
	vector<Dialogue> tempDialogue;
	//the gatekeeper
	tempDialogue = {
		{0, "Any Questions?"},
		{2, "What do you want to know?"},
		{5, "What's inside here"},
		{6, "Tips for beating this dungeon"},
		{7, "Do you work here"},
		{30, "Typical stuff. Some enemies, treasures, beat the boss. That's basically all."},
		{31, "There's a shop by the way, you should check it out."},
		{40, "I'm nowhere near a veteran in fighting, however, I do know one or two tips."},
		{41, "We use EXP system in this place. When you defeat an enemy, you gain some."},
		{42, "Your LEVEL increases when you gain a certain amount of EXP, which strengthens you power."},
		{43, "They also drop coins. Use them to buy some upgrades."},
		{50, "I wouldn't call this a job, they don't pay me."},
		{51, "But someone needs to watch the entrance, right?"},
		{100, "Well, good luck in there."}
	};
	tempNPC.push_back(NPC("The Gatekeeper", 0, tempShelf, tempDialogue));
	tempDialogue.clear();
	//Trader
	tempShelf = {
		{10, Item("Canned Fruit", -1, 30, 0, 0, 0, 3), 15, 1},
		{11, Item("Abandoned Sword", -1, 0, 20, 0, 0, 1, 1), 60, 0},
		{12, Item("Poisonous Arrow", -1, 0, 15, 0, 0, 1, 2), 60, 0},
		{13, Item("Lighting Wand", -1, 0, 5, 0, 0, 1, 3), 70, 0},
		{14, Item("Reinforced Armor", -1, 0, 30, 60, 0, 2), 105, 0}
	};
	tempDialogue = {
		{0, "Good day! How can I help you?"},
		{1, "Then you come to the right place! We got the finest stuff in this dungeon."},
		{2, "Sure! I love chatting with people!"},
		{5, "Why do you open a shop here"},
		{6, "How do you survive in this place"},
		{7, "Your hobbies"},
		{10, "These were entirely grown by myself, they might save you from emergency."}, //Canned Fruit
		{11, "Someone left this beside my front door, it's still a decent weapon though."}, //Abandoned Sword
		{12, "Its tip is soaked with Viper venom, surely my most polular product."}, //Poisonous Arrow
		{13, "I'm not sure how you can summon lightning from the roof, but hey, it works."}, //Lighting Wand
		{14, "Made from the best smith in the world, 100% bulletproof and weight like a feather."}, //Reinforced Armor
		{20, "Glad you like it! "}, //deal
		{21, "You seem to be running out of money. Earn more then come back."}, //no deal
		{22, "But I'm not sure if you know how to use it."}, //not match
		{23, "And you look like the perfect owner of it!"}, //match
		{30, "Very good question. So a long time ago, I was also a challenger, just like you."},
		{31, "I was sort of tired of fighting , so I found this safe spot and opened a shop."},
		{32, "So you new players can take some break here."},
		{40, "Everytime my health is low, I'll have some of these fruits."},
		{41, "My HP will instantly increase. They're truly a lifesaver."},
		{42, "You should grab some as well. Not for free of course."},
		{50, "You know, there's really not much you can do in one small room."},
		{51, "I like to sing though, it's the best way to kill the time."},
		{52, "Not now however, maybe when you defeat the boss."},
		{100, "Have a nice day, folks!"}
	};
	tempNPC.push_back(NPC("Trader", 6, tempShelf, tempDialogue));
	tempShelf.clear();
	tempDialogue.clear();
	//Smith
	tempDialogue = {
		{0, "...what's up?"},
		{2, "sure, i guess."},
		{5, "How much else till the boss room"},
		{6, "Are you trapped in here"},
		{30, "not sure, but you're more than half way there."},
		{31, "that doesn't mean it's easier though."},
		{40, "not trapped, just failed to escape."},
		{41, "uhh i mean unable to leave."},
		{42, "i mean forced to stay."},
		{43, "..."},
		{44, "ok yes trapped."},
		{100, "alright."}
	};
	tempNPC.push_back(NPC("Smith", 9, tempShelf, tempDialogue));
	tempNPC.push_back(NPC("?", 16, tempShelf, tempDialogue));
	return tempNPC;
}

void NPC::loadCheck(int _id) {
	extern NPC* npcLoad;
	if (getRoomID() == _id) {
		npcLoad = this;
	}
}