const http = require('http');
const url = require('url');

const server = http.createServer((req, res) => {
    const { method, url: reqUrl, headers } = req;
    const parsedUrl = url.parse(reqUrl, true);

    const logEntry = {
        time: new Date().toISOString(),
        ip: headers['x-forwarded-for'] || req.socket.remoteAddress,
        method,
        path: parsedUrl.pathname,
        query: parsedUrl.query,
        userAgent: headers['user-agent'],
        referrer: headers['referer'] || 'direct'
    };

    console.log(JSON.stringify(logEntry));

    // Set CORS headers
    res.setHeader('Access-Control-Allow-Origin', '*');
    res.setHeader('Access-Control-Allow-Methods', 'GET, POST, PUT, DELETE, OPTIONS');
    res.setHeader('Access-Control-Allow-Headers', 'Content-Type');

    if (method === 'OPTIONS') {
        res.writeHead(200);
        res.end();
        return;
    }

    // Health check endpoint
    if (parsedUrl.pathname === '/health') {
        res.writeHead(200, { 'Content-Type': 'application/json' });
        res.end(JSON.stringify({
            status: 'healthy',
            uptime: process.uptime(),
            timestamp: new Date().toISOString()
        }));
        return;
    }

    // Metrics endpoint for Prometheus
    if (parsedUrl.pathname === '/metrics') {
        const uptime = process.uptime();
        const memUsage = process.memoryUsage();

        const metrics = `
# HELP nodejs_app_uptime_seconds Process uptime in seconds
# TYPE nodejs_app_uptime_seconds counter
nodejs_app_uptime_seconds ${uptime}

# HELP nodejs_app_memory_usage_bytes Memory usage in bytes
# TYPE nodejs_app_memory_usage_bytes gauge
nodejs_app_memory_usage_bytes{type="rss"} ${memUsage.rss}
nodejs_app_memory_usage_bytes{type="heapTotal"} ${memUsage.heapTotal}
nodejs_app_memory_usage_bytes{type="heapUsed"} ${memUsage.heapUsed}
nodejs_app_memory_usage_bytes{type="external"} ${memUsage.external}
        `.trim();

        res.writeHead(200, { 'Content-Type': 'text/plain' });
        res.end(metrics);
        return;
    }

    // Default response
    res.writeHead(200, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify({
        status: 'logged',
        message: 'Request logged successfully',
        timestamp: new Date().toISOString()
    }));
});

server.listen(3000, () => {
    console.log(JSON.stringify({
        time: new Date().toISOString(),
        level: 'info',
        message: 'HTTP server running on port 3000'
    }));
});

// Graceful shutdown
process.on('SIGTERM', () => {
    console.log(JSON.stringify({
        time: new Date().toISOString(),
        level: 'info',
        message: 'Received SIGTERM, shutting down gracefully'
    }));
    server.close(() => {
        process.exit(0);
    });
});