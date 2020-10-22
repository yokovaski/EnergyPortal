<template>
  <v-app>
    <v-app-bar
        elevate-on-scroll
        app 
        color="blue-grey darken-3" 
        dark>
      <v-container class="py-0 fill-height">
        <v-btn text to="/">Home</v-btn>
        <v-btn text to="/about">About</v-btn>

        <v-spacer></v-spacer>

        <v-btn
            href="https://github.com/vuetifyjs/vuetify/releases/latest"
            target="_blank"
            text
        >
          <span class="mr-2">Latest Release</span>
          <v-icon>mdi-open-in-new</v-icon>
        </v-btn>
      </v-container>
    </v-app-bar>

    <v-main class="blue-grey lighten-5">
      <v-container>
        <router-view></router-view>
      </v-container>
    </v-main>
  </v-app>
</template>
