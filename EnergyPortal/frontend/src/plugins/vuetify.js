import Vue from 'vue';
import Vuetify from 'vuetify/lib/framework';

Vue.use(Vuetify);

const vuetify = new Vuetify({
    theme: {
        themes: {
            primary: "#00bcd4",
            secondary: "#e91e63",
            accent: "#cddc39",
            error: "#f44336",
            warning: "#ffc107",
            info: "#607d8b",
            success: "#8bc34a"
        }
    }
});

export default vuetify;
