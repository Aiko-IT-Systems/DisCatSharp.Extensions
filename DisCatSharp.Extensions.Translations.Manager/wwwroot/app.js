(() => {
	const PRIMARY_LOCALE = "en-US";
	const FALLBACK_LOCALES = [
		"da", "de", "en-GB", "en-US", "es-ES", "fr", "hr", "it", "lt", "hu", "nl", "no",
		"pl", "pt-BR", "ro", "fi", "sv-SE", "vi", "tr", "cs", "el", "bg", "ru", "uk",
		"hi", "th", "zh-CN", "ja", "zh-TW", "ko"
	];
	const params = new URLSearchParams(location.search);
	const storedScheme = localStorage.getItem("vscodeScheme");
	let vscodeScheme = params.get("vscodeScheme")
		|| (storedScheme === "vscode-insiders" || storedScheme === "vscode"
			? storedScheme
			: (navigator.userAgent.includes("Insider") ? "vscode-insiders" : "vscode"));

	const defaultRepoBlobBase = "https://github.com/Aiko-IT-Systems/DisCatSharp.Extensions/blob/main";
	const storedRepoBlobBase = localStorage.getItem("repoBlobBase");
	let repoBlobBase = params.get("gitBase") || storedRepoBlobBase || defaultRepoBlobBase;
	localStorage.setItem("repoBlobBase", repoBlobBase);

	const storedVscodeBase = localStorage.getItem("vscodeBase");
	let vscodeBase = params.get("vscodeBase") || storedVscodeBase;
	let customVscodeBase = !!vscodeBase;
	if (!vscodeBase)
		vscodeBase = `${vscodeScheme}://file/`;
	const state = {
		report: null,
		strings: {},
		locales: [],
		selected: null,
	};

	const el = (id) => document.getElementById(id);
	const keyList = el("keyList");
	const details = el("detailsBody");
	const summary = el("summary");
	const scheme = el("scheme");
	const linkConfigBtn = document.createElement("button");
	linkConfigBtn.id = "linkConfig";
	linkConfigBtn.textContent = "Link Settings";
	linkConfigBtn.className = "secondary";
	scheme?.parentElement?.append(linkConfigBtn);
	const search = el("search");
	const filter = el("filter");
	const beautifyBtn = el("beautify");
	const downloadBtn = el("downloadStrings");
	const modalHost = el("modalHost");

	document.getElementById("refresh").addEventListener("click", refreshAll);
	document.getElementById("addKey").addEventListener("click", onAddKey);
	document.getElementById("beautify").addEventListener("click", beautify);
	downloadBtn.addEventListener("click", downloadStrings);
	scheme.addEventListener("change", () => {
		vscodeScheme = scheme.value;
		localStorage.setItem("vscodeScheme", vscodeScheme);
		if (!customVscodeBase)
		{
			vscodeBase = `${vscodeScheme}://file/`;
			localStorage.setItem("vscodeBase", vscodeBase);
		}
		renderDetails(state.selected);
	});

	linkConfigBtn.addEventListener("click", configureLinks);

	// initialize scheme selector based on detection
	scheme.value = vscodeScheme;
	search.addEventListener("input", renderKeyList);
	filter.addEventListener("change", renderKeyList);

	async function loadAll() {
		const [report, strings, locales] = await Promise.all([
			fetchJson("/api/report"),
			fetchJson("/api/strings"),
			fetchJson("/api/locales"),
		]);
		state.report = report;
		state.strings = strings;
		state.locales = (locales && locales.length > 0) ? locales : FALLBACK_LOCALES;
		summary.textContent = `Used ${report.usedKeysCount} / Defined ${report.definedKeysCount} | Missing ${report.missingKeys.length} | Unused ${report.unusedKeys.length} | Dynamic ${report.dynamicKeyUsages.length}`;
		renderKeyList();
		renderDetails(state.selected);
	}

	function renderKeyList() {
		if (!state.report) return;
		const query = search.value.toLowerCase();
		const filterValue = filter.value;

		const translationKeys = Object.keys(state.strings);
		const dynamicKeys = (state.report.dynamicKeyUsages || [])
			.map((d) => keyFromDynamic(d))
			.filter((k) => k && k.includes("."));
		const keys = Array.from(new Set([...translationKeys, ...dynamicKeys])).sort((a, b) => a.localeCompare(b));
		keyList.innerHTML = "";
		const template = document.getElementById("key-item-template");

		keys.forEach((key) => {
			const translations = state.strings[key] || {};
			const valuesText = Object.values(translations)
				.join(" ")
				.toLowerCase();
			if (
				query &&
				!key.toLowerCase().includes(query) &&
				!valuesText.includes(query)
			)
				return;

			const isUnused = state.report.unusedKeys.includes(key);
			const isMissing = Object.values(translations).some((v) => !v);
			const isDynamic = state.report.dynamicKeyUsages.some(
				(d) => keyFromDynamic(d) === key
			);
			const isDynamicOnly = isDynamic && !state.report.keyUsages[key];

			if (filterValue === "missing" && !isMissing) return;
			if (filterValue === "unused" && !isUnused) return;
			if (filterValue === "dynamic" && !isDynamic) return;

			const node = template.content.firstElementChild.cloneNode(true);
			node.querySelector(".key-text").textContent = key;
			const badges = node.querySelector(".badges");
			if (isUnused) badges.append(makeBadge("Unused", "warn"));
			if (isMissing) badges.append(makeBadge("Missing", "danger"));
			if (isDynamic) badges.append(makeBadge("Dynamic", "success"));
			node.addEventListener("click", () => renderDetails(key));
			if (state.selected === key) node.classList.add("selected");
			keyList.append(node);
		});
	}

	function renderDetails(key) {
		state.selected = key;
		if (!key) {
			details.className = "details-empty";
			details.textContent = "Select a key to edit";
			return;
		}
		const translations = state.strings[key] || {};
		const locales = orderLocales(Object.keys(translations));

		const wrapper = document.createElement("div");
		wrapper.className = "details";
		const title = document.createElement("div");
		title.innerHTML = `<strong>${key}</strong>`;
		wrapper.append(title);

		const transWrap = document.createElement("div");
		transWrap.className = "translations";
		locales.forEach((locale) => {
			transWrap.append(makeLocaleRow(locale, translations[locale] ?? ""));
		});
		const addLocaleRow = document.createElement("div");
		addLocaleRow.className = "translation-add-row";
		const addSelect = document.createElement("select");
		const availableLocales = orderLocales(state.locales);
		const existing = new Set(locales);
		const options = availableLocales.filter((l) => !existing.has(l));
		addSelect.innerHTML = `<option value="">Select locale</option>${options
			.map((l) => `<option value="${l}">${l}</option>`)
			.join("")}`;
		const addButton = document.createElement("button");
		addButton.textContent = "Add locale";
		addButton.className = "secondary";
		addButton.onclick = () => {
			const loc = addSelect.value;
			if (!loc) return;
			if (transWrap.querySelector(`[data-locale="${loc}"]`)) return;
			transWrap.insertBefore(makeLocaleRow(loc, ""), addLocaleRow);
			addSelect.value = "";
		};
		addLocaleRow.append(addSelect, addButton);
		transWrap.append(addLocaleRow);
		wrapper.append(transWrap);

		const usageSection = document.createElement("div");
		usageSection.className = "usage";
		usageSection.innerHTML = "<strong>Usages</strong>";
		const usageList = document.createElement("div");
		const usages = state.report.keyUsages[key] || [];
		const dynUsages = (state.report.dynamicKeyUsages || []).filter(
			(d) => keyFromDynamic(d) === key
		);
		const usageTemplate = document.getElementById("usage-row-template");
		const repoRoot = state.report.repoRoot.replace(/\\/g, "/");
		if (usages.length > 0) {
			usages.forEach((u) => {
				const row =
					usageTemplate.content.firstElementChild.cloneNode(true);
				row.querySelector(
					".usage-file"
				).textContent = `${u.file} : ${u.line}`;
				const fullPath = `${repoRoot}/${u.file}`.replace(/\\/g, "/");
				const openBtn = ensureUsageOpen(row);
				openBtn.onclick = () => {
					const codeUrl = `${vscodeBase}${encodeURI(fullPath)}:${u.line}`;
					window.open(codeUrl, "_blank");
				};
				const gitBtn = row.querySelector(".usage-copy");
				const gitUrl = `${repoBlobBase}/${u.file.replace(/\\/g, "/")}#L${u.line}`;
				gitBtn.onclick = () => copyText(gitUrl, "Git link copied");
				usageList.append(row);
			});
		}
		if (dynUsages.length > 0) {
			dynUsages.forEach((d) => {
				const row = usageTemplate.content.firstElementChild.cloneNode(true);
				row.querySelector(".usage-file").textContent = `${d.file} : ${d.line} (dynamic: ${d.reason})`;
				const fullPath = `${repoRoot}/${d.file}`.replace(/\\/g, "/");
				const openBtn = ensureUsageOpen(row);
				openBtn.onclick = () => {
					const codeUrl = `${vscodeBase}${encodeURI(fullPath)}:${d.line}`;
					window.open(codeUrl, "_blank");
				};
				const gitBtn = row.querySelector(".usage-copy");
				const gitUrl = `${repoBlobBase}/${d.file.replace(/\\/g, "/")}#L${d.line}`;
				gitBtn.onclick = () => copyText(gitUrl, "Git link copied");
				usageList.append(row);
			});
		}
		if (usages.length === 0 && dynUsages.length === 0) {
			const none = document.createElement("div");
			none.textContent = "No usages found.";
			none.className = "details-empty";
			usageList.append(none);
		}
		usageSection.append(usageList);
		wrapper.append(usageSection);

		const actions = document.createElement("div");
		actions.className = "footer-actions";
		const saveBtn = document.createElement("button");
		saveBtn.textContent = "Save";
		saveBtn.classList.add("success");
		saveBtn.onclick = () => saveKey(key, transWrap);
		const deleteBtn = document.createElement("button");
		deleteBtn.textContent = "Delete";
		deleteBtn.className = "danger";
		deleteBtn.onclick = () => deleteKey(key);
		actions.append(saveBtn, deleteBtn);
		wrapper.append(actions);

		details.className = "";
		details.innerHTML = "";
		details.append(wrapper);
	}

	function makeLocaleRow(locale, value) {
		const row = document.createElement("div");
		row.className = "translation-row";
		row.dataset.locale = locale;
		const input = document.createElement("input");
		input.value = locale;
		input.disabled = true;
		const textarea = document.createElement("textarea");
		textarea.value = value ?? "";
		autosizeText(textarea);
		textarea.addEventListener("input", () => autosizeText(textarea));
		const remove = document.createElement("button");
		remove.textContent = "Remove";
		remove.className = "danger";
		remove.onclick = () => {
			if (locale === PRIMARY_LOCALE) return;
			row.remove();
		};
		if (locale === PRIMARY_LOCALE) {
			remove.disabled = true;
			remove.textContent = "Primary";
		}
		row.append(input, textarea, remove);
		return row;
	}

	function autosizeText(textarea) {
		textarea.style.height = "auto";
		textarea.style.height = `${textarea.scrollHeight}px`;
	}

	function orderLocales(locales) {
		return [...locales].sort((a, b) => {
			if (a === b) return 0;
			if (a === PRIMARY_LOCALE) return -1;
			if (b === PRIMARY_LOCALE) return 1;
			return a.localeCompare(b);
		});
	}

	async function saveKey(key, transWrap) {
		const ok = await confirmModal("Save changes?", "This will write updates to strings.json.");
		if (!ok) return;
		const translations = {};
		transWrap.querySelectorAll(".translation-row").forEach((row) => {
			const loc = row.dataset.locale;
			const text = row.querySelector("textarea").value;
			if (loc) translations[loc] = text;
		});
		const res = await fetch(`/api/strings/${encodeURIComponent(key)}`, {
			method: "PUT",
			headers: { "Content-Type": "application/json" },
			body: JSON.stringify({ key, translations }),
		});
		if (!res.ok) {
			const msg = await readError(res);
			alert(`Save failed: ${msg}`);
			return;
		}
		await loadAll();
		renderDetails(key);
		showToast("Saved");
	}

	async function deleteKey(key) {
		const ok = await confirmModal("Delete key?", `This removes '${key}' from strings.json.`);
		if (!ok) return;
		const res = await fetch(`/api/strings/${encodeURIComponent(key)}`, { method: "DELETE" });
		if (!res.ok) {
			const msg = await readError(res);
			alert(`Delete failed: ${msg}`);
			return;
		}
		await loadAll();
		renderDetails(null);
		showToast("Deleted");
	}

	async function onAddKey() {
		const key = prompt("New translation key");
		if (!key) return;
		const res = await fetch("/api/strings", {
			method: "POST",
			headers: { "Content-Type": "application/json" },
			body: JSON.stringify({ key, translations: { "en-US": "" } }),
		});
		if (!res.ok) {
			const msg = await readError(res);
			alert(`Add failed: ${msg}`);
			return;
		}
		await loadAll();
		renderDetails(key);
	}

	async function beautify() {
		const ok = await confirmModal("Beautify strings.json?", "This will rewrite strings.json with formatting.");
		if (!ok) return;
		const res = await fetch('/api/strings/format', { method: 'POST' });
		if (!res.ok) {
			const msg = await readError(res);
			alert(`Beautify failed: ${msg}`);
			return;
		}
		await loadAll();
		showToast('Formatted');
	}

	function makeBadge(text, tone) {
		const span = document.createElement("span");
		span.className = `badge ${tone}`;
		span.textContent = text;
		return span;
	}

	function keyFromDynamic(dyn) {
		if (!dyn) return "";
		const raw = dyn.expression || "";
		const normalized = normalizeDynamicKey(raw);
		return normalized || raw || "";
	}

	function normalizeDynamicKey(expr) {
		let s = expr.trim();
		// Remove leading interpolation markers
		if (s.startsWith("@$")) s = s.slice(2);
		else if (s.startsWith("$")) s = s.slice(1);
		// Strip surrounding quotes
		s = s.replace(/^"/, "").replace(/"$/, "");
		// Cut at first interpolation
		const brace = s.indexOf("{");
		if (brace >= 0) s = s.slice(0, brace);
		return s.trim();
	}

	function ensureUsageOpen(row) {
		let btn = row.querySelector(".usage-open");
		if (!btn) {
			btn = document.createElement("button");
			btn.className = "usage-open";
			btn.textContent = "Open in VS Code";
			const actions = row.querySelector(".usage-actions") || row;
			actions.prepend(btn);
		}
		return btn;
	}

	async function refreshAll() {
		try {
			await loadAll();
			showToast("Refreshed");
		} catch (err) {
			const message = err?.message || err;
			alert(`Refresh failed: ${message}`);
		}
	}

	async function downloadStrings() {
		try {
			const res = await fetch("/api/strings");
			if (!res.ok) throw new Error(await readError(res));
			const data = await res.json();
			const blob = new Blob([JSON.stringify(data, null, 2)], { type: "application/json" });
			const url = URL.createObjectURL(blob);
			const a = document.createElement("a");
			a.href = url;
			a.download = "strings.json";
			a.click();
			URL.revokeObjectURL(url);
			showToast("Downloaded strings.json");
		} catch (err) {
			alert(`Download failed: ${err}`);
		}
	}

	function copyText(text, toastMessage) {
		navigator.clipboard?.writeText(text).then(() => showToast(toastMessage));
	}

	function configureLinks() {
		const gitInput = prompt("Git blob base (e.g. https://github.com/org/repo/blob/main)", repoBlobBase);
		if (gitInput) {
			repoBlobBase = gitInput.trim().replace(/\/$/, "");
			localStorage.setItem("repoBlobBase", repoBlobBase);
		}

		const vscodeInput = prompt("VS Code URI base (leave blank to follow scheme)", customVscodeBase ? vscodeBase : "");
		if (vscodeInput !== null) {
			const trimmed = vscodeInput.trim();
			if (trimmed) {
				vscodeBase = trimmed.endsWith("/") ? trimmed : `${trimmed}/`;
				customVscodeBase = true;
				localStorage.setItem("vscodeBase", vscodeBase);
			} else {
				customVscodeBase = false;
				vscodeBase = `${vscodeScheme}://file/`;
				localStorage.setItem("vscodeBase", vscodeBase);
			}
		}

		renderDetails(state.selected);
	}

	function showToast(message) {
		const toast = document.createElement("div");
		toast.className = "toast";
		toast.textContent = message;
		document.body.append(toast);
		setTimeout(() => toast.remove(), 2000);
	}

	function confirmModal(title, message) {
		return new Promise((resolve) => {
			const backdrop = document.createElement("div");
			backdrop.className = "modal-backdrop";
			const modal = document.createElement("div");
			modal.className = "modal";
			modal.innerHTML = `<h3>${title}</h3><p>${message}</p>`;
			const actions = document.createElement("div");
			actions.className = "modal-actions";
			const cancel = document.createElement("button");
			cancel.className = "ghost";
			cancel.textContent = "Cancel";
			const ok = document.createElement("button");
			ok.className = "danger";
			ok.textContent = "Confirm";
			cancel.onclick = () => { backdrop.remove(); resolve(false); };
			ok.onclick = () => { backdrop.remove(); resolve(true); };
			actions.append(cancel, ok);
			modal.append(actions);
			backdrop.append(modal);
			modalHost.append(backdrop);
		});
	}

	async function fetchJson(url) {
		const res = await fetch(url);
		if (!res.ok) {
			const msg = await readError(res);
			throw new Error(msg);
		}
		return res.json();
	}

	async function readError(res) {
		try {
			const data = await res.json();
			if (data?.message) return data.message;
			return `${res.status} ${res.statusText}`;
		} catch {
			return `${res.status} ${res.statusText}`;
		}
	}

	loadAll();
})();
