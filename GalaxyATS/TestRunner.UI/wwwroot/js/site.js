window.ResultUpdater = (runId) => {
    function poll(fn) {
        let pollOnPage = 10 * 1000;
        let pollOffPage = 60 * 1000;
        let pollTime = document.hasFocus() ? pollOnPage : pollOffPage;

        let sleep = () => new Promise(resolve => {
            let sleepStart = new Date();
            let sleepCheck = () => {
                let timeDiff = new Date() - sleepStart;
                if (timeDiff >= pollTime) {
                    resolve();
                }
                else {
                    setTimeout(sleepCheck, 1000);
                }
            };
            setTimeout(sleepCheck, 0);
        });
        let poller = () => {
            new Promise((resolve, reject) => {
                fn(resolve, reject);
            }).then(
                () => sleep().then(poller),
                () => console.log('result polling stopped'));
        };
        setTimeout(poller, 0);

        window.addEventListener("focus", () => {
            pollTime = pollOnPage;
        }, false);

        window.addEventListener("blur", () => {
            pollTime = pollOffPage;
        }, false);
    }

    function escape(string) {
        let htmlEscapes = {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#x27;',
            '/': '&#x2F;'
        };

        let htmlEscaper = /[&<>"'\/]/g;
        return ('' + string).replace(htmlEscaper, function (match) {
            return htmlEscapes[match];
        });
    }

    let passedCount = document.querySelector('#passed');
    let startedCount = document.querySelector('#started');
    let blockedCount = document.querySelector('#blocked');
    let queuedCount = document.querySelector('#queued');
    let failedCount = document.querySelector('#failedTests');
    let passedTable = document.querySelector('#passedTable');
    let startedTable = document.querySelector('#startedTable');
    let blockedTable = document.querySelector('#blockedTable');
    let queuedTable = document.querySelector('#queuedTable');
    let failedTable = document.querySelector('#failedInfoTable');
    let endTimeSpan = document.querySelector('#endTime');
    let testListInput = document.querySelector('input[name="testList"]');

    function updatePie(data) {
        window.myPie.data.datasets.forEach((dataset) => {
            dataset.data = data;
        });
        window.myPie.update();
    }

    function updatePassed(passedTests) {
        let table = `
                    <tbody>
                        ${passedTests.map(passedTest =>
            `<tr class="infoTableRow">
                                <td class="infoRow2">
                                    <span><a class="commitLinks" href="/RunResult/${runId}/TestResult/${passedTest.testId}">${passedTest.name.substring(0, Math.min(passedTest.name.length, 55))}${passedTest.name.length > 55 ? "..." : ""}</a></span>
                                </td>
                            </tr>`).join("")}
                    </tbody>`;
        passedTable.innerHTML = table;
        passedCount.innerText = passedCount.innerText.replace(/\d+/g, passedTests.length);
    }

    function updateFailed(failedTests, failedTestsByMessageLists) {
        let table = `
                    <tbody>
                        ${failedTests.map(failedTest =>
            `<tr class="infoTableRow listAll">
                                <td class="failedTestRow2 hidden">
                                    <span class="failedTestName"><a class="failedTestLink" href="/RunResult/${runId}/TestResult/${failedTest.testId}">${failedTest.name.substring(0, Math.min(failedTest.name.length, 100))}${failedTest.name.length > 100 ? "..." : ""}</a></span>
                                    <br />
                                    <span class="failedType">${failedTest.errorType}</span>
                                    <br />
                                    <span class="failedText">${escape(failedTest.message)}</span>
                                </td>
                            </tr>`).join("")}
                       ${Object.entries(failedTestsByMessageLists).map(([key, failedTestsByMessageList]) =>
                `<tr class="infoTableRow groupedByMessage">
                                <td class="failedTestRow2">
                                    <span class="failedTestName">${failedTestsByMessageList[0].errorType}: </span><span class="failedType">${escape(key)}</span>
                                    <ul>
                                        ${failedTestsByMessageList.map(failedTest =>
                    `<li>
                                                <span class="failedTestName"><a class="failedTestLink" href="/RunResult/${runId}/TestResult/${failedTest.testId}" title="${failedTest.name}">${failedTest.name.substring(0, Math.min(failedTest.name.length, 95))}${failedTest.name.length > 95 ? "..." : ""}</a></span>
                                                ${failedTest.url ?
                        `<ul>
                                                        <li>
                                                            <span class="failedText">${failedTest.url}</span>
                                                        </li>
                                                    </ul>` : ''}
                                            </li>`).join("")}
                                    </ul>
                                </td>
                            </tr>`).join("")}
                    </tbody>`;
        failedTable.innerHTML = table;
        failedCount.innerText = failedCount.innerText.replace(/\d+/g, failedTests.length);
    }

    function updateTabs(queuedTests, blockedTests, startedTests) {
        let mappings = [
            [queuedTable, queuedTests, queuedCount],
            [blockedTable, blockedTests, blockedCount],
            [startedTable, startedTests, startedCount]
        ];

        for (let i = 0; i < 3; ++i) {
            let mapping = mappings[i];
            let tableElement = mapping[0];
            let testNames = mapping[1];
            let countElement = mapping[2];
            let table = `
                        <tbody>
                            ${testNames.map(testName =>
                `<tr class="infoTableRow">
                                    <td class="infoRow2">
                                        <span>${testName}</span>
                                    </td>
                                </tr>`).join("")}
                        </tbody>`;
            tableElement.innerHTML = table;
            countElement.innerText = countElement.innerText.replace(/\d+/g, testNames.length);
        }
    }

    function updateEndTime(endTime) {
        let date = new Date(endTime);
        if (date.getYear() > 0) {
            let dtf = new Intl.DateTimeFormat('en',
                { year: 'numeric', month: 'numeric', day: '2-digit', hour: 'numeric', minute: '2-digit', second: '2-digit', hour12: true });
            let [{ value: mo }, , { value: da }, , { value: ye }, , { value: hr }, , { value: mn }, , { value: sc }, , { value: dp }] = dtf.formatToParts(date);
            endTimeSpan.innerText = `${mo}/${da}/${ye} ${hr}:${mn}:${sc} ${dp}`;
            endTimeSpan.setAttribute("datetime", date.toISOString());
        }
    }

    function updateNonPassedTestList(testList) {
        testListInput.value = testList;
    }

    if (new Date(endTimeSpan.getAttribute('datetime')).getYear() < 0) {
        poll((next, stop) => {
            if ('myPie' in window) {
                let r = new XMLHttpRequest();
                r.open('GET', `/RunResultJson/${runId}`, true);
                r.onreadystatechange = () => {
                    if (r.readyState === 4 && r.status === 200) {
                        let response = JSON.parse(r.responseText);
                        if (response !== null) {
                            updatePie([
                                response.passedTests.length,
                                response.failedTests.length,
                                response.blockedTests.length,
                                response.queuedTests.length,
                                response.startedTests.length
                            ]);

                            updateEndTime(response.endTime);
                            updatePassed(response.passedTests);
                            updateTabs(response.queuedTests, response.blockedTests, response.startedTests);
                            updateFailed(response.failedTests, response.failedTestsByMessage);
                            updateNonPassedTestList(response.nonPassedTests);

                            if (response.startedTests.length === 0 && response.queuedTests.length === 0) {
                                stop();
                            } else {
                                next();
                            }
                        } else {
                            next();
                        }
                    }
                };
                r.send();
            }
            else {
                next();
            }
        });
    }
};