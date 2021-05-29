<template>
    <v-app>
        <v-app-bar
            clipped-left
            app
            color="blue-grey darken-3"
            dark
        >
            <div class="d-flex align-center">
                <v-btn
                    text
                    active-class=""
                    @click.prevent="goToHome"
                >
                    <v-img
                        :src="require('./assets/energiezicht.svg')"
                        class="my-3"
                        contain
                        width="35"
                    />

                    <h2 class="ml-3">EnergieZicht</h2>
                </v-btn>
            </div>

            <v-spacer></v-spacer>
            <v-menu
                bottom
                left
                offset-y
            >
                <template v-slot:activator="{ on, attrs }">
                    <v-btn
                        icon
                        v-bind="attrs"
                        v-on="on"
                    >
                        <v-icon>mdi-account</v-icon>
                    </v-btn>
                </template>
                <v-list
                    nav
                    dense
                >
                    <v-list-item link to="/">
                        <v-list-item-icon>
                            <v-icon>mdi-account-cog</v-icon>
                        </v-list-item-icon>
                        <v-list-item-title>Profiel</v-list-item-title>
                    </v-list-item>
                    <v-list-item link @click="logout">
                        <v-list-item-icon>
                            <v-icon>mdi-logout-variant</v-icon>
                        </v-list-item-icon>
                        <v-list-item-title>Uitloggen</v-list-item-title>
                    </v-list-item>
                </v-list>
            </v-menu>
        </v-app-bar>
        <v-navigation-drawer
            v-if="!onMobile"
            app
            clipped
            permanent
        >
            <v-list
                nav
                dense
            >
                <v-list-item link to="/">
                    <v-list-item-icon>
                        <v-icon>mdi-home-analytics</v-icon>
                    </v-list-item-icon>
                    <v-list-item-title>Actueel</v-list-item-title>
                </v-list-item>
                <v-list-item link to="/history">
                    <v-list-item-icon>
                        <v-icon>mdi-history</v-icon>
                    </v-list-item-icon>
                    <v-list-item-title>Historie</v-list-item-title>
                </v-list-item>
            </v-list>
            <template v-slot:append>
                <v-divider></v-divider>
                <v-list
                    nav
                    dense
                >
                    <v-list-item link to="/settings">
                        <v-list-item-icon>
                            <v-icon>mdi-cog-outline</v-icon>
                        </v-list-item-icon>
                        <v-list-item-title>Instellingen</v-list-item-title>
                    </v-list-item>
                </v-list>
            </template>
        </v-navigation-drawer>

        <v-main class="main-background">
            <router-view/>
        </v-main>
        <v-bottom-navigation 
            v-if="onMobile"
            grow
            shift
            fixed
            hide-on-scroll
            color="primary"
        >
            <v-btn link to="/">
                <span>Actueel</span>

                <v-icon>mdi-home-analytics</v-icon>
            </v-btn>
            <v-btn link to="/history">
                <span>Historie</span>

                <v-icon>mdi-history</v-icon>
            </v-btn>

            <v-btn link to="/settings">
                <span>Instellingen</span>

                <v-icon>mdi-cog-outline</v-icon>
            </v-btn>
        </v-bottom-navigation>
    </v-app>
</template>

<script>

import Axios from "axios";

export default {
    name: 'App',

    computed: {
        onMobile() {
            return this.$vuetify.breakpoint.mdAndDown;
        }
    },
    
    data: () => ({
        drawer: true,
        group: null,
    }),

    watch: {
        group () {
            this.drawer = false
        },
        $route: {
            immediate: true,
            handler(to) {
                document.title = to.name || 'EnergieZicht';
            }
        },
    },
    
    methods: {
        goToHome() {
            if (this.$route.name === 'Actueel') {
                return;
            }
            
            this.$router.push({ path: '/' });
        },
        async logout() {
            try {
                await Axios.get('/webapi/v3/session/logout');
                window.location = '';
            } catch (e) {
                console.error(e);
            }
        }
    }
};
</script>

<style type="text/css">
.main-background {
    background: #ebeef5;
}
</style>
