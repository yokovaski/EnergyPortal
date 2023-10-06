<template>
  <div class="w-100 dropdown">
    <b-link
        v-show="inputHovered"
        class="input-select-arrow"
    >
      <font-awesome-icon
          icon="fa-solid fa-sort-down"
          @mouseover="selectArrowHovered = true"
      ></font-awesome-icon>
    </b-link>
    <input
        ref="autocompleteInput"
        class="form-control"
        type="text"
        placeholder="Select a time zone"
        @click="open = true"
        @focusout="open = suggestionsFocused"
        :value="value"
        @input="updateValue($event.target.value)"
        @keydown.enter = 'enter'
        @keydown.down = 'down'
        @keydown.up = 'up'
        @keydown.esc="escape"
        @mouseover="inputHovered = true"
        @mouseleave="inputHovered = selectArrowHovered"
    >
    <div
        class="dropdown-content w-100"
        v-bind:class="{ show : open }"
        v-bind:style="{ height: suggestionsHeight + 'px' }"
        @mouseover="suggestionsFocused = true"
        @mouseleave="suggestionsFocused = false"
    >
      <div ref="suggestionsContainer" class="w-100 dropdown-content-scrollable">
        <a
            ref="suggestions"
            v-for="(suggestion, index) in matches"
            :key="index"
            v-bind:class="{'active': isActive(index)}"
            @click="suggestionClick(suggestion)"
            href="#"
            @mouseover="current = index"
        >
          {{ suggestion.id }}
        </a>
        <a
            v-if="numberOfMatches === 0"
            class="text-muted"
        >
          No suggestions...
        </a>
      </div>
    </div>
  </div>
</template>

<script>
function isElementInViewport(el) {
  let rect = el.getBoundingClientRect();
  return (
      rect.top >= 0 &&
      rect.left >= 0 &&
      rect.bottom <= (window.innerHeight || document. documentElement.clientHeight) &&
      rect.right <= (window.innerWidth || document. documentElement.clientWidth)
  );
}

export default {
  name: 'AutoComplete',
  props: {
    suggestions: {
      type: Array,
      required: true
    },
    preFilledValue: {
      type: String,
      required: false,
      default: ''
    }
  },
  data () {
    return {
      open: false,
      current: 0,
      numberOfMatches: 0,
      value: '',
      suggestionsFocused: false,
      inputHovered: false,
      selectArrowHovered: false
    }
  },
  computed: {
    // Filtering the suggestion based on the input
    matches() {
      let matches = this.suggestions.filter(suggestion => suggestion.id.toLowerCase().match(this.value.toLowerCase()));
      this.numberOfMatches = matches.length;

      return matches;
    },
    suggestionsHeight() {
      if (this.numberOfMatches > 8)
        return 8 * 48;
      else if (this.numberOfMatches > 0)
        return this.numberOfMatches * 48;

      return 48;
    },
    openSuggestion () {
      return this.selection !== '' &&
          this.matches.length !== 0 &&
          this.open === true;
    }
  },
  mounted() {
    this.value = this.preFilledValue;
  },
  methods: {
    testingIn() {
      console.log('focus in');
    },
    testingOut() {
      console.log('focus out');
    },
    updateValue (value) {
      if (this.open === false) {
        this.open = true;
        this.current = 0;
      }

      this.value = value;
      this.current = 0;

      if (this.suggestions.some(suggestion => suggestion.id === value))
        this.$emit('input', value)
    },
    // When enter pressed on the input
    enter () {
      let suggestion = this.matches[this.current];
      this.value = suggestion.id;
      this.$refs.autocompleteInput.blur();
      this.$emit('input', suggestion);
    },
    // When up pressed while suggestions are open
    up () {
      if (this.current > 0) {
        this.current--;

        let element = this.$refs.suggestions[this.current];

        let elTop = element.getBoundingClientRect().top;
        let parentTop = this.$refs.suggestionsContainer.getBoundingClientRect().top;

        if (elTop < parentTop)
          element.scrollIntoView();
      }
    },
    // When up pressed while suggestions are open
    down () {
      if (this.current < this.matches.length - 1) {
        this.current++;

        let element = this.$refs.suggestions[this.current];

        let elBottom = element.getBoundingClientRect().bottom;
        let parentBottom = this.$refs.suggestionsContainer.getBoundingClientRect().bottom;

        if (elBottom > parentBottom)
          element.scrollIntoView(false);
      }
    },
    escape() {
      this.value = '';
      this.open = false;
      this.current = 0;
      this.$refs.autocompleteInput.blur();
    },
    // For highlighting element
    isActive (index) {
      return index === this.current;
    },
    // When one of the suggestion is clicked
    suggestionClick (suggestion) {
      this.value = suggestion.id;
      this.$emit('input', suggestion);
      this.open = false;
    }
  }
}
</script>

<style scoped>
.dropdown {
  position: relative;
  display: inline-block;
}

/* Dropdown Content (Hidden by Default) */
.dropdown-content {
  display: none;
  position: absolute;
  background-color: #f5f8fa;
  box-shadow: 0 8px 16px 0 rgba(0,0,0,0.2);
  z-index: 1;
  border-radius: 0.25rem;
}

/* Dropdown Content (Hidden by Default) */
.dropdown-content-scrollable {
  height: 100%;
  overflow-y: auto;
  border-radius: 0.25rem;
}

/* Links inside the dropdown */
.dropdown-content a {
  color: black;
  padding: 12px 16px;
  text-decoration: none;
  display: block;
}

.active {
  background-color: #ddd;
}

.input-select-arrow {
  color: black;
  position: absolute;
  z-index: 1;
  top: 5px;
  right: 15px;
}

/* Change color of dropdown links on hover */
.dropdown-content a:hover {background-color: #ddd}

/* Show the dropdown menu (use JS to add this class to the .dropdown-content container when the user clicks on the dropdown button) */
.show {display:block;}
</style>