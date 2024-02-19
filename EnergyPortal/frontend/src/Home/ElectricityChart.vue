<template>
  <v-card class="pa-2">
    <div
        class="text-subtitle-1 text-center text-grey-darken-2"
        v-text="chartName"
    ></div>
    <apexchart :series="chartData.series" :options="chartData.chartOptions"></apexchart>
    <v-btn-toggle
        mandatory
        v-model="selectedRange"
        tile
        group
        class="align-center"
    >
      <v-btn>Nu</v-btn>
      <v-btn>Uur</v-btn>
      <v-btn>Dag</v-btn>
      <v-btn>Week</v-btn>
    </v-btn-toggle>
  </v-card>
</template>

<script>
export default {
  name: 'ElectricityChart',
  props: ['chartName', 'chartData', 'activeRange'],
  data: () => ({
    toggle: null,
    selectedRange: 2,
    options: {
      legend: {
        display: false
      },
      maintainAspectRatio: false,
      scales:{
        xAxes: [{
          ticks: {
            autoSkip: true,
            maxTicksLimit: 10
          }
        }]
      },
      responsive: false
    },
    settingNewRange: false
  }),
  watch: {
    activeRange() {
      if (this.settingNewRange) {
        this.settingNewRange = false;
        return;
      }
      this.settingNewRange = true;

      switch (this.activeRange) {
        case "now":
          this.selectedRange = 0;
          break;
        case "hour":
          this.selectedRange = 1;
          break;
        case "day":
          this.selectedRange = 2;
          break;
        case "week":
          this.selectedRange = 3;
          break;
      }
    },
    selectedRange() {
      if (this.settingNewRange) {
        this.settingNewRange = false;
        return;
      }

      this.settingNewRange = true;
      switch (this.selectedRange) {
        case 0:
          this.$emit('change-range', 'now')
          break;
        case 1:
          this.$emit('change-range', 'hour')
          break;
        case 2:
          this.$emit('change-range', 'day')
          break;
        case 3:
          this.$emit('change-range', 'week')
          break;
      }
    }
  }
};
</script>

<style scoped>
</style>