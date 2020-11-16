<template>
  <v-card class="pa-2">
    <div
        class="text-subtitle-1 text-center grey--text"
        v-text="chartName"
    ></div>
    <line-chart class="energy-chart" ref="lineChart"
                :chart-data="chartData"
                :options="options">
    </line-chart>
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
import LineChart from './LineChart';

export default {
  name: 'ElectricityChart',
  components: { LineChart },
  props: ['chartName', 'chartData', 'activeRange'],
  data: function() {
    return {
      selectedRange: 1,
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
        }
      },
      settingNewRange: false
    }
  },
  methods: {
    emitNewRange() {
      this.sendingNewRange = true;
      
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
    },
    refresh() {
      this.$refs.lineChart.refresh();
    }
  },
  watch: {
    chartData: {
      deep: true,
      handler () {
        // console.log('detected change in EnergyChart');
      }
    },
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
    // activeRange() {
    //     console.log('Active range changed');
    //     this.$refs.lineChart.refresh();
    // }
  }
};
</script>

<style scoped>
.energy-chart {
  height: 250px;
}
</style>