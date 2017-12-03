new Vue({
    el: '#root',
    data: {
        reports: []
    },
    mounted () {
        axios.get('/api/location')
            .then((response) => {
                this.reports = response.data;
                console.log(response);
            });
    }
});