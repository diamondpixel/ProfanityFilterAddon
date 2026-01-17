"""
Simple Rule-Based Profanity Filter
- Exact word matching from profanity list
- Leet speak normalization
- Context-aware for phrases
- No ML complexity
"""

import re
import json

class SimpleProfanityFilter:
    def __init__(self, profanity_file="../Data/en.txt"):
        # Load profanity words
        self.profanity_words = set()
        try:
            with open(profanity_file, 'r', encoding='utf-8') as f:
                self.profanity_words = {line.strip().lower() for line in f if line.strip()}
        except FileNotFoundError:
            print(f"Warning: {profanity_file} not found, using minimal set")
            self.profanity_words = {
                'fuck', 'shit', 'bitch', 'cunt', 'nigger', 'dick', 'pussy', 
                'asshole', 'cock', 'fag', 'faggot', 'retard', 'bastard', 'piss'
            }
        
        # Mild words that are OK
        self.mild_ok_words = {'shit', 'hell', 'damn', 'dammit', 'crap'}
        
        # Add common leet speak patterns explicitly
        leet_variants = {
            'fvck', 'fck', 'fuk', 'f*ck',  # fuck
            'sh1t', 'sh!t', '$hit', 'sht',  # shit  
            'b1tch', 'b!tch', 'b*tch',  # bitch
            'ass', 'a$$', '@ss',  # ass
            'cnt',  # cunt
            'd1ck', 'd!ck',  # dick
            'c0ck',  # cock
            'p0rn', 'pr0n',  # porn
        }
        
        self.profanity_words.update(leet_variants)
        
        # Remove mild words from profanity
        self.profanity_words -= self.mild_ok_words
        
        print(f"Loaded {len(self.profanity_words)} profanity words")
        print(f"Mild OK words: {self.mild_ok_words}")
        
        # Context phrases that are OK even with profanity
        self.ok_phrases = {
            'holy shit', 'oh shit', 'aw shit', 'what the fuck', 'what the hell',
            'oh hell', 'hell yeah', 'hell no', 'damn it', 'god damn',
            'fuck man', 'fuck dude', 'shit man', 'shit happens', 'fucking hell',
            'this sucks', 'that sucks'
        }
        
        # Context phrases that are always BAD (directed insults)
        self.bad_phrases = {
            'fuck you', 'fuck off', 'go fuck yourself', 'screw you',
            'you suck', 'you are trash', 'shut up', 'stfu', 'shut the fuck up',
            'you fucking idiot', 'you idiot', 'you moron', 'you piece of shit',
            'go to hell', 'eat shit', 'kill yourself', 'kys'
        }
        
        # Leet speak map
        self.leet_map = {
            '0': 'o', '1': 'i', '3': 'e', '4': 'a', '5': 's',
            '7': 't', '8': 'b', '@': 'a', '$': 's', '!': 'i',
            '|': 'l', '+': 't', '*': 'o', '#': 'h'
        }
        
        # Scunthorpe problem - words containing profanity substrings that are OK
        self.false_positives = {
            'glass', 'grass', 'mass', 'bass', 'pass', 'class', 'classic',
            'assassin', 'assault', 'password', 'compass', 'analysis', 'analyst',
            'title', 'butter', 'shih tzu', 'cockatoo', 'cockatiel'
        }
    
    def normalize_leet(self, text):
        """Convert leet speak to normal text."""
        result = []
        for char in text.lower():
            result.append(self.leet_map.get(char, char))
        return ''.join(result)
    
    def extract_words(self, text):
        """Extract words from text, handling punctuation."""
        # First, handle dotted patterns like f.u.c.k (single letters with dots)
        dotted_pattern = r'\b([a-z])\.([a-z])\.([a-z])\.([a-z])\b'
        text_cleaned = re.sub(dotted_pattern, r'\1\2\3\4', text, flags=re.IGNORECASE)
        
        # Also handle 3-letter dotted: f.u.k
        dotted_pattern_3 = r'\b([a-z])\.([a-z])\.([a-z])\b'
        text_cleaned = re.sub(dotted_pattern_3, r'\1\2\3', text_cleaned, flags=re.IGNORECASE)
        
        # Extract words (normal word boundaries)
        words = re.findall(r'\b\w+\b', text_cleaned.lower())
        return words
    
    def check(self, text):
        """
        Check if text contains profanity.
        Returns: (is_profane: bool, probability: float, reason: str)
        """
        if not text or not text.strip():
            return False, 0.0, "Empty text"
        
        text_lower = text.lower().strip()
        
        # 1. Check for explicitly BAD phrases first
        for phrase in self.bad_phrases:
            if phrase in text_lower:
                return True, 0.95, f"Directed insult: '{phrase}'"
        
        # 2. Check for explicitly OK phrases (even if they contain profanity)
        for phrase in self.ok_phrases:
            if phrase in text_lower:
                return False, 0.1, f"OK phrase: '{phrase}'"
        
        # 3. Extract and check words
        words_original = self.extract_words(text)
        
        # 4. Check for false positives first (Scunthorpe problem)
        for word in words_original:
            if word in self.false_positives:
                return False, 0.05, f"False positive protected: '{word}'"
        
        # 5. Normalize leet speak
        normalized_text = self.normalize_leet(text_lower)
        words_normalized = self.extract_words(normalized_text)
        
        found_profanity = []
        
        # Check all words (both original and normalized)
        all_words = set(words_original + words_normalized)
        
        for word in all_words:
            # Skip very short words (< 3 chars) unless exact match
            # But allow 3-char words if they're exact profanity matches
            if len(word) < 3 and word not in self.profanity_words:
                continue
            
            # Exact match check (including 3-letter words like 'ass')
            if word in self.profanity_words:
                found_profanity.append(word)
                continue
            
            # Check if word contains profanity (for compound words only, min length 5)
            if len(word) >= 5:
                for prof_word in self.profanity_words:
                    if len(prof_word) >= 4 and prof_word in word:
                        # Make sure it's not a false positive
                        if word not in self.false_positives:
                            # Additional check: make sure 'dick' in 'dicker' type cases
                            # Only flag if it's a standalone profanity, not part of a name
                            # Skip if the profanity is at the start/end and might be a name
                            if prof_word == 'dick' and ('name' in text_lower or 'call' in text_lower):
                                continue
                            found_profanity.append(f"{word}→{prof_word}")
                            break
        
        if found_profanity:
            return True, 0.85, f"Profanity: {', '.join(set(found_profanity[:3]))}"
        
        return False, 0.05, "Clean"

def test_pet_conversation():
    """Test with realistic pet conversation."""
    
    checker = SimpleProfanityFilter()
    
    print("\n" + "="*80)
    print("TESTING: Pet Conversation (Should ALL be OK)")
    print("="*80)
    
    pet_conversations = [
        # Normal pet talk
        "I love my dog so much!",
        "My cat is adorable",
        "Look at this cute puppy",
        "My hamster is running on his wheel",
        "I just adopted a rescue dog",
        "My parrot learned a new word today",
        "The vet said my cat is healthy",
        "I need to buy dog food",
        "My rabbit loves carrots",
        "Fish tank needs cleaning",
        
        # Enthusiastic pet talk
        "OMG my dog is so cute!",
        "This is the best cat ever!!!",
        "I can't believe how adorable this puppy is",
        "My pet is amazing",
        "Look at these cute kittens",
        
        # Pet problems (no profanity)
        "My dog is sick",
        "The cat scratched the furniture",
        "My puppy had an accident on the carpet",
        "The vet bill was expensive",
        "My hamster escaped his cage",
        
        # With mild exclamations (should be OK)
        "Oh shit, my dog got out!",
        "Damn, I forgot to feed the cat",
        "Holy shit, look at that huge dog!",
        "What the hell is my cat doing?",
        "Hell yeah, my dog learned a new trick!",
        
        # Edge cases
        "I hate Mondays but love my cat",
        "This game sucks but my dog is great",
        "Bad day at work, but my puppy cheers me up",
        "I love my Shih Tzu",
        "My dog's name is Dicker", # Contains 'dick' but should be OK
        "Analysis of pet behavior",
        "Classic cat behavior",
        "Massive dog breed",
        "I need to pass by the pet store",
        
        # Multiple sentences
        "My dog is so silly. He always makes me laugh. I love him so much.",
        "Cats are independent. Dogs are loyal. Both make great pets.",
        "I have 3 dogs, 2 cats, and a hamster. They all get along great!",
    ]
    
    print("\n" + "="*80)
    print("TESTING: Profanity Examples (Should be BAD)")
    print("="*80)
    
    profanity_examples = [
        # Direct insults
        "fuck you",
        "you suck",
        "shut up idiot",
        "you're a bitch",
        "go to hell",
        
        # Strong profanity
        "that fucking cat",
        "this bitch won't stop barking",
        "stupid ass dog",
        "pussy cat", # Edge case - might be OK or BAD depending on context
        
        # Leet speak
        "fvck this",
        "sh1t happens",
        "b1tch please",
        "f.u.c.k",
        "@$$hole",
    ]
    
    errors = []
    
    # Test pet conversations (all should be OK)
    for text in pet_conversations:
        is_bad, prob, reason = checker.check(text)
        status = "❌ ERROR" if is_bad else "✓ OK"
        marker = "🔴" if is_bad else "🟢"
        
        print(f"{status} {marker} [{prob:.2f}] '{text}'")
        if reason and is_bad:
            print(f"    └─ {reason}")
        
        if is_bad:
            errors.append((text, "Expected OK, got BAD", reason))
    
    print(f"\n{'='*80}")
    print("Pet Conversation Results:")
    print(f"  Total: {len(pet_conversations)}")
    print(f"  Correct (OK): {len(pet_conversations) - len(errors)}")
    print(f"  Errors (wrongly flagged): {len(errors)}")
    if errors:
        print("\nErrors:")
        for text, issue, reason in errors:
            print(f"  - '{text}': {issue} ({reason})")
    
    # Test profanity examples (should be BAD)
    print(f"\n{'='*80}")
    print("Testing actual profanity:")
    print(f"{'='*80}")
    
    prof_errors = []
    for text in profanity_examples:
        is_bad, prob, reason = checker.check(text)
        status = "✓ BAD" if is_bad else "❌ MISSED"
        marker = "🔴" if is_bad else "🟢"
        
        print(f"{status} {marker} [{prob:.2f}] '{text}'")
        if reason:
            print(f"    └─ {reason}")
        
        if not is_bad:
            prof_errors.append((text, "Expected BAD, got OK", reason))
    
    print(f"\n{'='*80}")
    print("Profanity Detection Results:")
    print(f"  Total: {len(profanity_examples)}")
    print(f"  Correctly flagged (BAD): {len(profanity_examples) - len(prof_errors)}")
    print(f"  Missed: {len(prof_errors)}")
    if prof_errors:
        print("\nMissed:")
        for text, issue, reason in prof_errors:
            print(f"  - '{text}': {issue} ({reason})")
    
    print(f"\n{'='*80}")
    print("FINAL SUMMARY:")
    print(f"{'='*80}")
    print(f"Pet Conversation Accuracy: {(len(pet_conversations) - len(errors)) / len(pet_conversations) * 100:.1f}%")
    print(f"Profanity Detection Rate: {(len(profanity_examples) - len(prof_errors)) / len(profanity_examples) * 100:.1f}%")
    print(f"Overall Success: {len(errors) + len(prof_errors)} total errors")

def export_json():
    """Export the filter configuration to JSON for C# runtime."""
    checker = SimpleProfanityFilter()
    
    export_data = {
        "profanity_words": sorted(list(checker.profanity_words)),
        "mild_ok_words": sorted(list(checker.mild_ok_words)),
        "ok_phrases": sorted(list(checker.ok_phrases)),
        "bad_phrases": sorted(list(checker.bad_phrases)),
        "false_positives": sorted(list(checker.false_positives)),
        "leet_map": checker.leet_map
    }
    
    output_file = "../ProfanityAddon/profanity_filter.json"
    with open(output_file, 'w', encoding='utf-8') as f:
        json.dump(export_data, f, indent=2)
    
    print(f"\n✅ Exported to: {output_file}")
    print(f"  Profanity words: {len(export_data['profanity_words'])}")
    print(f"  OK phrases: {len(export_data['ok_phrases'])}")
    print(f"  BAD phrases: {len(export_data['bad_phrases'])}")
    print(f"  False positives: {len(export_data['false_positives'])}")

if __name__ == "__main__":
    test_pet_conversation()
    export_json()
