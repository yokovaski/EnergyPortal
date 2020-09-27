<template>
    <div>
        <div class="row">
            <div v-if="selectedRange !== null" class="col-12">
                <b-dropdown :text="selectedRange.text" right class="float-right mb-3">
                    <b-dropdown-item v-for="range in defaultRangeOptions" @click="setSelectedRange(range)" href="#">{{ range.text }}</b-dropdown-item>
                </b-dropdown>
                
                <b-dropdown variant="outline-secondary" v-if="customEnabled" :text="groupByOptions[oldGroupBy]" right class="float-right mb-3 mr-2">
                    <b-dropdown-item v-for="(text, groupBy) in groupByOptions" @click="selectedRange.groupBy = groupBy" href="#">{{ text }}</b-dropdown-item>
                </b-dropdown>
            </div>
            <div v-if="customEnabled" class="col-12">
                <div class="row">
                    <div class="col-12 col-sm-6">
                        <b-form-group
                                id="from"
                                label="From"
                                label-for="from-datepicker"
                        >
                            <b-form-datepicker 
                                    id="from-datepicker" 
                                    class="mb-1"
                                    v-model="fromDate"
                            ></b-form-datepicker>
                            <b-form-timepicker
                                    hourCycle="h23"
                                    reset-button
                                    v-model="fromTime"
                            ></b-form-timepicker>
                        </b-form-group>
                    </div>
                    <div class="col-12 col-sm-6">
                        <b-form-group
                                id="to"
                                label="To"
                                label-for="from-datepicker"
                        >
                            <b-form-datepicker 
                                    id="to-datepicker" 
                                    class="mb-1"
                                    v-model="toDate"
                            ></b-form-datepicker>
                            <b-form-timepicker
                                    now-button
                                    reset-button
                                    v-model="toTime"
                            ></b-form-timepicker>
                        </b-form-group>
                    </div>
                </div>
            </div>
            <div class="col-12">
                <div class="card shadow-sm mb-3">
                    <div class="card-body">
                        <bar-chart ref="electricityChart" :chart-data="chartData" :options="options"></bar-chart>
                    </div>
                </div>
            </div>
            <div class="col-12">
                <div class="card shadow-sm">
                    <div class="card-body">
                        <bar-chart ref="gasChart" :chart-data="gasChartData" :options="options"></bar-chart>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
    import Axios from "axios"
    import EnergyChart from "../Home/EnergyChart";
    import BarChart from "./BarChart"
    import moment from "moment-timezone";
    import deepCopy from "../deepCopy";

    let colorDictionary = {
        usage: '#007bff',
        solar: '#ffc107',
        redelivery: '#28a745',
        intake: '#dc3545',
        gas: '#dc3545'
    };
    
    let initialEnergyCharts = {
        usage: {
            index: 0,
            chartName: 'Verbruik',
            backEndName: 'usage',
            borderColor: '#007bff',
            backgroundColor: 'rgba(0,137,255,0.26)'
        },
        solar: {
            index: 0,
            chartName: 'Opwekking',
            backEndName: 'solar',
            borderColor: '#ffc107',
            backgroundColor: 'rgba(255,193,7,0.26)'
        }
    };
    
    export default {
        name: "",
        components: {EnergyChart, BarChart},
        data: () => ({
            defaultRangeOptions: [
                {
                    text: 'Last day',
                    groupBy: 'hours',
                    amount: 24
                },
                {
                    text: 'Last week',
                    groupBy: 'days',
                    amount: 6
                },
                {
                    text: 'Last month',
                    groupBy: 'days',
                    amount: 30
                },
                {
                    text: 'Last year',
                    groupBy: 'months',
                    amount: 12
                },
                {
                    text: 'Last 5 years',
                    groupBy: 'years',
                    amount: 5
                },
                {
                    text: 'Select range',
                    groupBy: null
                }
            ],
            selectedRange: null,
            oldGroupBy: null,
            groupByOptions: {
                tenseconds: 'Per ten seconds',
                minutes: 'Per minute',
                hours: 'Per hour',
                days: 'Per day',
                weeks: 'Per week',
                months: 'Per month',
                years: 'Per year',
            },
            chartData: {
                labels: [],
                datasets: [
                    {
                        label: 'Verbruik',
                        backgroundColor: '#007bff',
                        data: []
                    },
                    {
                        label: 'Opwekking',
                        backgroundColor: '#ffc107',
                        data: []
                    },
                    {
                        label: 'Teruglevering',
                        backgroundColor: '#28a745',
                        data: []
                    },
                    {
                        label: 'Opname',
                        backgroundColor: '#dc3545',
                        data: []
                    },
                ]
            },
            gasChartData: {
                labels: [],
                datasets: [
                    {
                        label: 'Gas',
                        backgroundColor: '#dc3545',
                        data: []
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false
            },
            fromDate: '',
            fromTime: '00:00:00',
            toDate: '',
            toTime: '',
            solarSystem: false
        }),
        async mounted () {
            this.selectedRange = this.defaultRangeOptions.find(r => r.text === 'Last week');
            await this.getUserSettings();
            
            if (!this.solarSystem) {
                this.chartData.datasets = [this.chartData.datasets[0]];
            }
        },
        methods: {
            async getUserSettings() {
                try {
                    let response = await Axios.get('webapi/v3/settings');
                    this.solarSystem = response.data.solarSystem;
                } catch (e) {
                    console.error(e);
                    this.makeToast('Failed to fetch user settings.', 'danger');
                }
            },
            async fetchHistory() {
                try {
                    let config = {
                        params: {
                            from: this.from ? this.from : moment().substract(30, 'days').format(),
                            to: this.to ? this.to : moment().format()
                        }
                    } 
                    
                    let response = await Axios.get(`/webapi/v3/metrics/${this.selectedRange.groupBy}`, config);
                    this.chartData.labels = response.data.timestamps;
                    this.gasChartData.labels = response.data.timestamps;
                    this.chartData.datasets[0].data = response.data.usage;
                    
                    if (this.solarSystem) {
                        this.chartData.datasets[1].data = response.data.solar;
                        this.chartData.datasets[2].data = response.data.redelivery;
                        this.chartData.datasets[3].data = response.data.intake;
                    }
                    
                    this.gasChartData.datasets[0].data = response.data.gas;
                    this.$refs.electricityChart.update();
                    this.$refs.gasChart.update();
                    console.log(response.data);
                } catch (e) {
                    console.error(e);
                    
                    if (e.response.status === 400)
                        this.makeToast(e.response.data, 'warning');
                    else 
                        this.makeToast('An error occured while fetching the history', 'danger')
                    
                }
            },
            setSelectedRange(range) {
                this.selectedRange = deepCopy(range);
            },
            makeToast(message, variant = null) {
                this.$bvToast.toast(message, {
                    variant: variant,
                    solid: true
                })
            }
        },
        computed:{
            from() {
                if (this.fromDate.length < 1)
                    return null;
                
                if (this.fromTime.length < 1)
                    return null;
                
                return `${this.fromDate}T${this.fromTime}${this.offset}`;
            },
            to() {
                if (this.toDate.length < 1)
                    return null;
                
                if (this.toTime.length < 1)
                    return null;
                
                return `${this.toDate}T${this.toTime}${this.offset}`;
            },
            offset() {
                let nowString = moment().tz('Europe/Amsterdam').format().split('T')[1];
                return nowString.substring(8);
            },
            customEnabled() {
                return (this.selectedRange && this.selectedRange.text === 'Select range') || false;
            },
            getFromMoment() {
                let from = moment();
                
                switch (this.selectedRange.groupBy) {
                    case 'minutes':
                        from.subtract(this.selectedRange.amount, 'minutes');
                        break;
                    case 'hours':
                        from.subtract(this.selectedRange.amount, 'hours');
                        break;
                    case 'days':
                    case 'weeks':
                        from.subtract(this.selectedRange.amount, 'days').startOf('day');
                        break;
                    case 'months':
                        from = from.subtract(this.selectedRange.amount, 'months').startOf('month');
                        from.add(1, 'months');
                        break;
                    case 'years':
                        from.subtract(this.selectedRange.amount, 'years').startOf('year');
                        from.add(1, 'years');
                        break;
                }
                
                return from;
            }
        },
        watch: {
            fromDate() {
                if (!this.customEnabled)
                    return;
                
                if (this.fromTime.length > 0)
                    this.fetchHistory(); 
            },
            fromTime() {
                if (!this.customEnabled)
                    return;
                
                if (this.fromDate.length > 0)
                    this.fetchHistory(); 
            },
            toDate() {
                if (!this.customEnabled)
                    return;
                
                if (this.toTime.length > 0)
                    this.fetchHistory(); 
            },
            toTime() {
                if (!this.customEnabled)
                    return;
                
                if (this.toDate.length > 0)
                    this.fetchHistory(); 
            },
            selectedRange: {
                async handler(range) {
                    if (range !== null)
                        console.log(`Range selection changed! new (${range.groupBy}), old (${this.oldGroupBy})`);

                    if (this.customEnabled) {
                        if (range.groupBy === null){
                            range.groupBy = this.oldGroupBy;
                            return;
                        }
                        
                        if (range.groupBy !== this.oldGroupBy)
                            await this.fetchHistory();

                        this.oldGroupBy = range.groupBy;
                        return;
                    } else {
                        this.toDate = '';
                        this.toTime = '';
                    }
                    
                    let from = this.getFromMoment.format().split('T');
                    this.fromDate = from[0];
                    this.fromTime = from[1].split('+')[0];

                    // if (subtractFormat === 'hours' || subtractFormat === 'minutes')
                    //     this.fromTime = from[1].split('+')[0];
                    // else
                    //     this.fromTime = '00:00:00';
                    
                    await this.fetchHistory();
                    this.oldGroupBy = range.groupBy;
                },
                deep: true
            }
        }
    }
</script>

<style scoped>

</style>