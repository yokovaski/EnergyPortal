<template>
  <v-app>
    <v-app-bar
        elevate-on-scroll
        app 
        color="blue-grey darken-3" 
        dark>
      <v-container class="py-0 fill-height">
        <v-btn text to="/">Actueel</v-btn>
        <v-btn text to="/about">Verleden</v-btn>
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
              <v-icon>mdi-dots-vertical</v-icon>
            </v-btn>
          </template>
          <v-list>
            <v-list-item>
              <v-list-item-title>
                <v-btn to="/" text style="text-transform: none !important;">Tijdzone</v-btn>
              </v-list-item-title>
            </v-list-item>
            <v-list-item>
              <v-list-item-title>Test</v-list-item-title>
            </v-list-item>
            <v-list-item>
              <v-list-item-title>Test</v-list-item-title>
            </v-list-item>
          </v-list>
        </v-menu>
      </v-container>
    </v-app-bar>

    <v-main class="blue-grey lighten-5">
      <v-container>
        <router-view></router-view>
      </v-container>
    </v-main>
  </v-app>
</template>
