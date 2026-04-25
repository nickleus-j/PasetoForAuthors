class WebCaller {
    constructor(baseUrl = "") {
        this.baseUrl = baseUrl;
    }

    async request({ url, method = "GET", data = null }) {
        const fullUrl = this.baseUrl + url;

        const options = {
            method,
            headers: { "Content-Type": "application/json; charset=utf-8" }
        };

        if (data && method !== "GET" && method !== "DELETE") {
            options.body = JSON.stringify(data);
        }

        // Handle query params for GET/DELETE
        const finalUrl =
            (method === "GET" || method === "DELETE") && data
                ? `${fullUrl}?${new URLSearchParams(data).toString()}`
                : fullUrl;

        try {
            const res = await fetch(finalUrl, options);
            if (!res.ok) {
                throw await res.json();
            }
            return await res.json();
        } catch (err) {
            throw err;
        }
    }

    // Convenience wrappers
    get(url, data) {
        return this.request({ url, method: "GET", data });
    }

    post(url, data) {
        return this.request({ url, method: "POST", data });
    }

    put(url, data) {
        return this.request({ url, method: "PUT", data });
    }

    delete(url, data) {
        return this.request({ url, method: "DELETE", data });
    }
}