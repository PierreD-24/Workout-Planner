document.addEventListener('DOMContentLoaded', function() {
    // Check if we're on the progress page
    const chartElement = document.getElementById('progress-chart');
    if (!chartElement || typeof window.progressData === 'undefined') {
        return; // Not on progress page or no data
    }

    console.log("Chart data available:", window.progressData.length + " items");
    
    try {
        // Make sure all libraries are loaded
        if (typeof React === 'undefined') {
            throw new Error("React library not loaded");
        }
        if (typeof ReactDOM === 'undefined') {
            throw new Error("ReactDOM library not loaded");
        }
        if (typeof Recharts === 'undefined') {
            throw new Error("Recharts library not loaded");
        }
        
        // Format the data
        const data = window.progressData.map(item => ({
            date: new Date(item.date).toLocaleDateString(),
            weight: parseFloat(item.weightUsed),
            volume: parseFloat(item.repsCompleted) * parseFloat(item.setsCompleted) * parseFloat(item.weightUsed)
        }));
        
        // Create the chart
        const { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } = Recharts;
        
        ReactDOM.render(
            React.createElement(
                'div',
                { className: 'chart-container' },
                React.createElement(
                    ResponsiveContainer,
                    { width: '100%', height: 400 },
                    React.createElement(
                        LineChart,
                        { data: data },
                        React.createElement(CartesianGrid, { strokeDasharray: '3 3' }),
                        React.createElement(XAxis, { dataKey: 'date' }),
                        React.createElement(YAxis),
                        React.createElement(Tooltip),
                        React.createElement(Legend),
                        React.createElement(Line, { 
                            type: 'monotone', 
                            dataKey: 'weight', 
                            stroke: '#8884d8', 
                            name: 'Weight' 
                        }),
                        React.createElement(Line, { 
                            type: 'monotone', 
                            dataKey: 'volume', 
                            stroke: '#82ca9d', 
                            name: 'Volume'
                        })
                    )
                )
            ),
            chartElement
        );
        
        console.log("Chart rendered successfully");
    } catch (error) {
        console.error("Error rendering chart:", error);
        chartElement.innerHTML = '<div class="alert alert-danger">Error rendering chart: ' + error.message + '</div>';
    }
});
